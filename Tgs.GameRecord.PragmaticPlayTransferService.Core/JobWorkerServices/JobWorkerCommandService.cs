using Microsoft.Extensions.Logging;
using Tgs.GameProviders.Client;
using Tgs.GameProviders.Domains;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.JobWorkerServices;

internal class JobWorkerCommandService : ICommandService<JobWorkerCommand>
{
    private readonly IGetService<GameRecordQuery, GameRecordResult> _gameRecordQueryService;
    private readonly ILogger<JobWorkerCommandService> _logger;
    private readonly IGameRecordScheduleService _scheduleService;
    private readonly ISystemClock _systemClock;
    private readonly IUtensil _utensil;

    public JobWorkerCommandService(
        IUtensil utensil,
        IGameRecordScheduleService scheduleService,
        IGetService<GameRecordQuery, GameRecordResult> gameRecordQueryService,
        ISystemClock systemClock,
        ILogger<JobWorkerCommandService> logger)
    {
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
        _scheduleService = scheduleService ?? throw new ArgumentNullException(nameof(scheduleService));
        _gameRecordQueryService = gameRecordQueryService ?? throw new ArgumentNullException(nameof(gameRecordQueryService));
        _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask ExecuteAsync(
        JobWorkerCommand command,
        CancellationToken cancellationToken = default)
    {
        var gameDataTypes = Enum.GetValues(typeof(PragmaticPlayTransferGameDataType)).Cast<PragmaticPlayTransferGameDataType>();

        var gameProviderId = await _utensil
            .GetGameProviderIdAsync()
            .ConfigureAwait(false);

        if (!gameProviderId.HasValue)
        {
            _logger.LogError("Can't find game provider id of {gpCode}", GameProviderType.PragmaticPlayTransfer);
            return;
        }

        foreach (var gameDataType in gameDataTypes)
        {
            try
            {
                var schedule = await _scheduleService
                                   .GetAsync(gameDataType, cancellationToken)
                                   .ConfigureAwait(false)
                               ?? await _scheduleService
                                   .InitAsync(gameProviderId.Value, gameDataType, cancellationToken)
                                   .ConfigureAwait(false);

                var sysTime = _systemClock.CurrentTime;

                while (schedule != null)
                {
                    var config = _scheduleService.GetConfig(schedule);

                    if (config.EndTime >= sysTime)
                        break;

                    var result = await _gameRecordQueryService
                        .GetAsync(new GameRecordQuery(gameDataType, config.StartTime), cancellationToken)
                        .ConfigureAwait(false);

                    if (result.Success)
                    {
                        schedule = await _scheduleService
                            .UpdateAsync(
                                schedule,
                                result.LastGameRecordTime,
                                result.NextTime!.Value,
                                sysTime,
                                cancellationToken)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發生例外");
            }
        }
    }
}
