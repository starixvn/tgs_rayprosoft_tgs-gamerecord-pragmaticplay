using Microsoft.Extensions.Caching.Memory;
using Tgs.GameProviders.QueryServer.Client;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

internal class PlayerService : IPlayerService
{
    private readonly IGameProvidersQueryServerClient _gameProvidersQueryServerClient;
    private readonly IMemoryCache _memoryCache;

    public PlayerService(
        IMemoryCache memoryCache,
        IGameProvidersQueryServerClient gameProvidersQueryServerClient)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _gameProvidersQueryServerClient = gameProvidersQueryServerClient ?? throw new ArgumentNullException(nameof(gameProvidersQueryServerClient));
    }

    public async ValueTask<PlayerInfo?> GetAsync(Guid gameProviderId, string playerCode)
    {
        if (_memoryCache.TryGetValue($"{gameProviderId}:{playerCode}", out PlayerInfo playerInfo))
        {
            return playerInfo;
        }

        var playerModel = await _gameProvidersQueryServerClient.PlayerGetAsync(playerCode).ConfigureAwait(false);

        if (playerModel == null)
        {
            return null;
        }

        var gameProviderConfigurationId = await _gameProvidersQueryServerClient
            .BrandGetGameProviderConfigurationIdAsync(playerModel.BrandId, gameProviderId).ConfigureAwait(false);

        if (!gameProviderConfigurationId.HasValue)
        {
            return null;
        }

        playerInfo = new PlayerInfo(
            playerModel.BrandId,
            playerModel.PlayerCode,
            playerModel.DefaultWalletCurrencyCode,
            playerModel.DefaultWalletId,
            gameProviderConfigurationId.Value);

        _ = _memoryCache.Set($"{gameProviderId}:{playerCode}", playerInfo, TimeSpan.FromHours(1));

        return playerInfo;
    }
}
