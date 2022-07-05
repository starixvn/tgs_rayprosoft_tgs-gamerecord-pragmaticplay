using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Tgs.GameProviders.QueryServer.Models;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

internal class UtensilMemoryCachedRepository : IUtensil
{
    private readonly TimeSpan _absoluteExpirationRelativeToNow;
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly IMemoryCache _memoryCache;
    private readonly IUtensil _utensil;

    public UtensilMemoryCachedRepository(
        IOptions<GameRecordInfoOptions> options,
        IUtensil utensil,
        IMemoryCache memoryCache)
    {
        _absoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
        _gameRecordInfo = options.Value;
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async ValueTask<Guid?> GetBrandGetGameProviderConfigurationIdAsync(Guid brandId, Guid gameProviderId)
    {
        var cacheKey = $"{brandId}:{gameProviderId}";

        if (_memoryCache.TryGetValue(cacheKey, out Guid? configurationId))
            return configurationId;

        configurationId = await _utensil
               .GetBrandGetGameProviderConfigurationIdAsync(brandId, gameProviderId)
               .ConfigureAwait(false);

        if (configurationId.HasValue)
        {
            _ = _memoryCache.Set(cacheKey, configurationId, _absoluteExpirationRelativeToNow);
        }

        return configurationId;
    }

    public async ValueTask<int?> GetGameIdAsync(Guid gameProviderId, string gameExternalId)
    {
        var cacheKey = $"{gameProviderId}:{gameExternalId}";

        if (_memoryCache.TryGetValue(cacheKey, out int? gameId))
            return gameId;

        gameId = await _utensil
            .GetGameIdAsync(gameProviderId, gameExternalId)
            .ConfigureAwait(false);

        if (gameId.HasValue)
        {
            _ = _memoryCache.Set(cacheKey, gameId, _absoluteExpirationRelativeToNow);
        }

        return gameId;
    }

    public async ValueTask<Guid?> GetGameProviderIdAsync()
    {
        var cacheKey = _gameRecordInfo.GameProviderCode;

        if (_memoryCache.TryGetValue(cacheKey, out Guid? gameProviderId))
            return gameProviderId;

        gameProviderId = await _utensil.GetGameProviderIdAsync()
                .ConfigureAwait(false);

        if (!gameProviderId.HasValue)
        {
            _ = _memoryCache.Set(cacheKey, gameProviderId, _absoluteExpirationRelativeToNow);
        }

        return gameProviderId;
    }

    public async ValueTask<PlayerModel?> GetPlayerAsync(string playerCode)
    {
        var cacheKey = playerCode;

        if (_memoryCache.TryGetValue(cacheKey, out PlayerModel? player))
            return player;

        player = await _utensil.GetPlayerAsync(playerCode)
            .ConfigureAwait(false);

        if (player is not null)
        {
            _ = _memoryCache.Set(cacheKey, player, _absoluteExpirationRelativeToNow);
        }

        return player;
    }
}
