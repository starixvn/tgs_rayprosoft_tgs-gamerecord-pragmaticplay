namespace Tgs.GameRecord.PragmaticPlay.Core;

public interface IHangfireLock
{
    public Task<bool> LockingAsync(string application, CancellationToken cancellationToken = default);
}
