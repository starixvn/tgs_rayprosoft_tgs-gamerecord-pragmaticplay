namespace Tgs.GameRecord.PragmaticPlay.Core;

public interface ISchedulePullRecordLock
{
    public Task<bool> LockingAsync(string dataType);

    public Task<bool> UnlockAsync(string dataType);
}
