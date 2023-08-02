using Tgs.GameRecord.PragmaticPlay.Core.JobApplicationServices;

namespace Tgs.GameRecord.PragmaticPlay.Core;

public interface IHangfireUtensil
{
    public Task JobCheckAsync();

    public Task JobExecuteAsync(JobExecuteCommand source);

    public Task JobInitAsync();
}
