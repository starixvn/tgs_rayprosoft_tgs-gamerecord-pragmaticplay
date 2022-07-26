using Tgs.GameRecord.PragmaticPlayTransferService.Core.JobWorkerServices;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core
{
    public interface IScheduleDispatch
    {
        public Task<bool> ExistAsync(string key);

        public Task<bool> LockingAsync(string key, JobWorkerCommand command);

        public Task<bool> UnlockAsync(string key);
    }
}
