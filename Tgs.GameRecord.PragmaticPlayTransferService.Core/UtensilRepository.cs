using Microsoft.Extensions.Options;
using Tgs.GameProviders.QueryServer.Client;
using Tgs.GameProviders.QueryServer.Models;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

internal class UtensilRepository : IUtensil
{
    private readonly IGameProvidersQueryServerClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfo;

    public UtensilRepository(
        IGameProvidersQueryServerClient client,
        IOptions<GameRecordInfoOptions> options)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _gameRecordInfo = options.Value;
    }

    public ValueTask<Guid?> GetBrandGetGameProviderConfigurationIdAsync(Guid brandId, Guid gameProviderId)
        => _client.BrandGetGameProviderConfigurationIdAsync(brandId, gameProviderId);

    public async ValueTask<int?> GetGameIdAsync(Guid gameProviderId, string gameExternalId)
    {
        var game = await _client.GameGetAsync(gameProviderId, gameExternalId).ConfigureAwait(false);
        return game?.GameId;
    }

    public ValueTask<Guid?> GetGameProviderIdAsync()
        => _client.GameProviderGetGameProviderIdAsync(_gameRecordInfo.GameProviderCode);

    public ValueTask<PlayerModel?> GetPlayerAsync(string playerCode)
        => _client.PlayerGetAsync(playerCode);
}
