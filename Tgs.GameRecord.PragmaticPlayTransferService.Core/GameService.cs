using Microsoft.Extensions.Caching.Memory;
using Tgs.GameProviders.QueryServer.Client;
using Tgs.GameProviders.QueryServer.Models;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

internal class GameService : IGameService
{
    private readonly IGameProvidersQueryServerClient _client;
    private readonly IMemoryCache _memoryCache;

    public GameService(
        IMemoryCache memoryCache,
        IGameProvidersQueryServerClient client)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask<int?> GetGameIdAsync(Guid gameProviderId, string gameExternalId)
    {
        var game = await GetGameAsync(gameProviderId, gameExternalId)
                .ConfigureAwait(false);

        return game?.GameId;
    }

    private async ValueTask<GameModel?> GetGameAsync(Guid gameProviderId, string gameExternalId)
    {
        var cacheKey = $"{gameProviderId}:{gameExternalId}";

        if (_memoryCache.TryGetValue(cacheKey, out GameModel? game))
        {
            return game;
        }

        game = await _client
            .GameGetAsync(gameProviderId, gameExternalId)
            .ConfigureAwait(false);

        if (game != null)
        {
            _ = _memoryCache.Set(cacheKey, game, TimeSpan.FromHours(1));
        }

        return game;
    }
}
