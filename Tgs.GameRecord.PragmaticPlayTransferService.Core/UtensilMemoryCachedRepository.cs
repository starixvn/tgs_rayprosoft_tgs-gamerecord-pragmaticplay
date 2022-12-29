using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Tgs.GameCommand.Common;

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

    public async ValueTask<Guid?> GetBrandGetGameProviderConfigurationIdAsync(
        Guid brandId,
        Guid gameProviderId,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{brandId}:{gameProviderId}";

        if (!_memoryCache.TryGetValue(cacheKey, out Guid? configurationId))
        {
            configurationId = await _utensil
                    .GetBrandGetGameProviderConfigurationIdAsync(
                        brandId,
                        gameProviderId,
                        cancellationToken)
                   .ConfigureAwait(false);

            if (configurationId.HasValue)
            {
                _ = _memoryCache.Set(cacheKey, configurationId, _absoluteExpirationRelativeToNow);
            }
        }
        return configurationId;
    }

    public async ValueTask<int?> GetGameIdAsync(
        Guid gameProviderId,
        string gameExternalId,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{gameProviderId}:{gameExternalId}";

        if (!_memoryCache.TryGetValue(cacheKey, out int? gameId))
        {
            gameId = await _utensil
                .GetGameIdAsync(
                    gameProviderId,
                    gameExternalId,
                    cancellationToken)
                .ConfigureAwait(false);

            if (gameId.HasValue)
            {
                _ = _memoryCache.Set(cacheKey, gameId, _absoluteExpirationRelativeToNow);
            }
        }
        return gameId;
    }

    public async ValueTask<Guid> GetGameProviderIdAsync(
        CancellationToken cancellationToken = default)
    {
        var cacheKey = _gameRecordInfo.GameProviderCode;

        if (!_memoryCache.TryGetValue(cacheKey, out Guid gameProviderId))
        {
            gameProviderId = await _utensil
                    .GetGameProviderIdAsync(
                        cancellationToken)
                    .ConfigureAwait(false);

            _ = _memoryCache.Set(cacheKey, gameProviderId);
        }

        return gameProviderId;
    }

    public async ValueTask<PlayerInfo?> GetPlayerAsync(
        string playerCode,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = playerCode;

        if (!_memoryCache.TryGetValue(cacheKey, out PlayerInfo? player))
        {
            player = await _utensil
                .GetPlayerAsync(
                    playerCode,
                    cancellationToken)
                .ConfigureAwait(false);

            if (player is not null)
            {
                _ = _memoryCache.Set(cacheKey, player, _absoluteExpirationRelativeToNow);
            }
        }

        return player;
    }
}
