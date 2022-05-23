using Microsoft.Extensions.Caching.Memory;
using Tgs.GameProviders.QueryServer.Models;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

internal class GameProvidersQueryMemoryCachedService : IGameProvidersQueryService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IGameProvidersQueryService _queryService;

    public GameProvidersQueryMemoryCachedService(
        IMemoryCache memoryCache,
        IGameProvidersQueryService queryService)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
    }

    public async ValueTask<Guid?> GetBrandGetGameProviderConfigurationIdAsync(Guid brandId, Guid gameProviderId)
    {
        var cacheKey = $"{brandId}:{gameProviderId}";

        if (_memoryCache.TryGetValue(cacheKey, out Guid? configurationId))
            return configurationId;

        configurationId = await _queryService
               .GetBrandGetGameProviderConfigurationIdAsync(brandId, gameProviderId)
               .ConfigureAwait(false);

        if (configurationId.HasValue)
        {
            _ = _memoryCache.Set(cacheKey, configurationId, TimeSpan.FromHours(1));
        }

        return configurationId;
    }

    public async ValueTask<int?> GetGameIdAsync(Guid gameProviderId, string gameExternalId)
    {
        var cacheKey = $"{gameProviderId}:{gameExternalId}";

        if (_memoryCache.TryGetValue(cacheKey, out int? gameId))
            return gameId;

        gameId = await _queryService
            .GetGameIdAsync(gameProviderId, gameExternalId)
            .ConfigureAwait(false);

        if (gameId.HasValue)
        {
            _ = _memoryCache.Set(cacheKey, gameId, TimeSpan.FromHours(1));
        }

        return gameId;
    }

    public async ValueTask<Guid?> GetGameProviderIdAsync(string gameProviderCode)
    {
        var cacheKey = gameProviderCode;

        if (_memoryCache.TryGetValue(cacheKey, out Guid? gameProviderId))
            return gameProviderId;

        gameProviderId = await _queryService.GetGameProviderIdAsync(gameProviderCode)
                .ConfigureAwait(false);

        if (!gameProviderId.HasValue)
        {
            _ = _memoryCache.Set(cacheKey, gameProviderId, TimeSpan.FromDays(1));
        }

        return gameProviderId;
    }

    public async ValueTask<PlayerModel?> GetPlayerAsync(string playerCode)
    {
        var cacheKey = playerCode;

        if (_memoryCache.TryGetValue(cacheKey, out PlayerModel? player))
            return player;

        player = await _queryService.GetPlayerAsync(playerCode)
            .ConfigureAwait(false);

        if (player is not null)
        {
            _ = _memoryCache.Set(cacheKey, player, TimeSpan.FromDays(1));
        }

        return player;
    }
}
