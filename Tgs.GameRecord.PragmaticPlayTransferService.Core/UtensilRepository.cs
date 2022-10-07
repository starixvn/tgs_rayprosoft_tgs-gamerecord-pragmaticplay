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

    public ValueTask<Guid?> GetBrandGetGameProviderConfigurationIdAsync(
        Guid brandId,
        Guid gameProviderId,
        CancellationToken cancellationToken = default)
        => _client
            .BrandGetGameProviderConfigurationIdAsync(
                brandId,
                gameProviderId,
                cancellationToken);

    public async ValueTask<int?> GetGameIdAsync(
        Guid gameProviderId,
        string gameExternalId,
        CancellationToken cancellationToken = default)
    {
        var game = await _client
            .GameGetAsync(
                gameProviderId,
                gameExternalId,
                cancellationToken)
            .ConfigureAwait(false);
        return game?.GameId;
    }

    public async ValueTask<Guid> GetGameProviderIdAsync(
        CancellationToken cancellationToken = default)
        => await _client
            .GameProviderGetGameProviderIdAsync(
                _gameRecordInfo.GameProviderCode,
                cancellationToken)
            .ConfigureAwait(false)
        ?? throw new Exception($"The identifier for this game provider does not exist.({_gameRecordInfo.GameProviderCode})");

    public ValueTask<PlayerModel?> GetPlayerAsync(
        string playerCode,
        CancellationToken cancellationToken = default)
        => _client
            .PlayerGetAsync(
                playerCode,
                cancellationToken);
}
