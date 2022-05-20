using Tgs.GameProviders.QueryServer.Client;
using Tgs.GameProviders.QueryServer.Models;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

internal class GameProvidersQueryService : IGameProvidersQueryService
{
    private readonly IGameProvidersQueryServerClient _client;

    public GameProvidersQueryService(
        IGameProvidersQueryServerClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public ValueTask<Guid?> GetBrandGetGameProviderConfigurationIdAsync(Guid brandId, Guid gameProviderId)
        => _client.BrandGetGameProviderConfigurationIdAsync(brandId, gameProviderId);

    public async ValueTask<int?> GetGameIdAsync(Guid gameProviderId, string gameExternalId)
    {
        var game = await _client.GameGetAsync(gameProviderId, gameExternalId).ConfigureAwait(false);
        return game?.GameId;
    }

    public ValueTask<Guid?> GetGameProviderIdAsync(string gameProviderCode)
        => _client.GameProviderGetGameProviderIdAsync(gameProviderCode);

    public ValueTask<PlayerModel?> GetPlayerAsync(string playerCode)
        => _client.PlayerGetAsync(playerCode);
}
