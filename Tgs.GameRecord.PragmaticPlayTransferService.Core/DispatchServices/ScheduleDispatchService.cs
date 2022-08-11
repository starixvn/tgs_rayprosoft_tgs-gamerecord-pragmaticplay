using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Tgs.GameProviders.Client;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.JobWorkerServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.DispatchServices;

internal class ScheduleDispatchService : ICommandService<ScheduleDispatchCommand>
{
    private readonly GameRecordInfoOptions _gameRecordInfoOptions;
    private readonly IGameRecordScheduleService _gameRecordScheduleService;
    private readonly IGetService<JobWorkerQuery, JobWorkerResult> _jobWorkerService;
    private readonly IScheduleDispatch _scheduleDispatch;
    private readonly ISystemClock _systemClock;

    public ScheduleDispatchService(
        IOptions<GameRecordInfoOptions> options,
        IGetService<JobWorkerQuery, JobWorkerResult> jobWorkerService,
        IScheduleDispatch scheduleDispatch,
        IGameRecordScheduleService gameRecordScheduleService,
        ISystemClock systemClock)
    {
        _gameRecordInfoOptions = options.Value;
        _jobWorkerService = jobWorkerService ?? throw new ArgumentNullException(nameof(jobWorkerService));
        _scheduleDispatch = scheduleDispatch ?? throw new ArgumentNullException(nameof(scheduleDispatch));
        _gameRecordScheduleService = gameRecordScheduleService ?? throw new ArgumentNullException(nameof(gameRecordScheduleService));
        _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
    }

    public async ValueTask ExecuteAsync(ScheduleDispatchCommand command, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse(command.GameDataType, out PragmaticPlayTransferGameDataType gameDataType))
        {
            return;
        }

        var key = $"GameRecord:PragmaticPlayService:{_gameRecordInfoOptions.GameProviderCode}:{command.GameDataType}".ToLower();
        if (!await _scheduleDispatch.ExistAsync(key: key).ConfigureAwait(false))
        {
            if (await _scheduleDispatch.LockingAsync(key: key, command: command).ConfigureAwait(false))
            {
                try
                {
                    var gameRecordSchedule =
                        await _gameRecordScheduleService.GetAsync(
                            gameDataType: gameDataType,
                            cancellationToken: cancellationToken).ConfigureAwait(false) ??
                        await _gameRecordScheduleService.InitAsync(
                            gameProviderId: command.GameProviderId,
                            gameDataType: gameDataType,
                            cancellationToken: cancellationToken).ConfigureAwait(false);

                    if (gameRecordSchedule == null)
                    {
                        return;
                    }

                    var config = _gameRecordScheduleService.GetConfig(gameRecordSchedule);

                    var startTime = gameDataType switch
                    {
                        PragmaticPlayTransferGameDataType.VirtualSports => config.NextTime.AddMinutes(-_gameRecordInfoOptions.VirtualSportsIntervalMinute),
                        PragmaticPlayTransferGameDataType.LiveDealer => config.NextTime.AddMinutes(-_gameRecordInfoOptions.LiveDealerIntervalMinute),
                        _ => config.NextTime
                    };

                    var jobWorkerQuery = new JobWorkerQuery(
                        command.GameProviderId,
                        command.GameProviderCode,
                        gameDataType,
                        StartTime: startTime,
                        EndTime: _systemClock.UtcNow);

                    var result = await _jobWorkerService
                            .GetAsync(
                                query: jobWorkerQuery,
                                cancellationToken: cancellationToken).ConfigureAwait(false);

                    _ = await _gameRecordScheduleService.UpdateAsync(
                        gameRecordSchedule: gameRecordSchedule,
                         gameDataType: gameDataType,
                         nextTime: result.NextTime,
                         lastGameRecordTime: result.LastGameRecordTime,
                         cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    await _scheduleDispatch.UnlockAsync(key: key).ConfigureAwait(false);
                }
            }
        }
    }
}
