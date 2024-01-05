using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Tgs.GameRecord.PragmaticPlay.Core;

internal class SchedulePullRecordLock(
    IOptions<GameRecordInfoOptions> options,
    IDatabase database) : ISchedulePullRecordLock
{
    private readonly GameRecordInfoOptions _gameRecordInfo = options.Value;

    public Task<bool> LockingAsync(string dataType)
        => database.StringSetAsync(key: $"{_gameRecordInfo.GameProviderCode.ToLower()}:schedulepullrecord:{dataType}", value: dataType, expiry: TimeSpan.FromMinutes(_gameRecordInfo.LockMinute), when: When.NotExists);

    public Task<bool> UnlockAsync(string dataType)
        => database.KeyDeleteAsync(key: $"{_gameRecordInfo.GameProviderCode.ToLower()}:schedulepullrecord:{dataType}");
}
