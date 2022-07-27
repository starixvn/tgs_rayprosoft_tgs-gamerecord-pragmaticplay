using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameProviders.Client;
using Tgs.GameProviders.Domains;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.JobWorkerServices;

internal class JobWorkerCommandService : ICommandService<JobWorkerCommand>
{
    private readonly IGetService<GameRecordQuery, GameRecordResult> _gameRecordQueryService;
    private readonly ILogger<JobWorkerCommandService> _logger;
    private readonly IScheduleDispatch _scheduleDispatch;
    private readonly ISystemClock _systemClock;
    private readonly IUtensil _utensil;
    private readonly GameRecordInfoOptions _gameRecordInfoOptions;

    public JobWorkerCommandService(
        IOptions<GameRecordInfoOptions> options,
        IUtensil utensil,
        IGetService<GameRecordQuery, GameRecordResult> gameRecordQueryService,
        ISystemClock systemClock,
        ILogger<JobWorkerCommandService> logger,
        IScheduleDispatch scheduleDispatch)
    {
        _gameRecordInfoOptions = options.Value;
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
        _gameRecordQueryService = gameRecordQueryService ?? throw new ArgumentNullException(nameof(gameRecordQueryService));
        _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scheduleDispatch = scheduleDispatch ?? throw new ArgumentNullException(nameof(scheduleDispatch));
    }

    public async ValueTask ExecuteAsync(
        JobWorkerCommand command,
        CancellationToken cancellationToken = default)
    {


        var gameProviderId = await _utensil
            .GetGameProviderIdAsync()
            .ConfigureAwait(false);

        if (!gameProviderId.HasValue)
        {
            _logger.LogError("Can't find game provider id of {gpCode}", GameProviderType.PragmaticPlayTransfer);
            return;
        }

        if (!Enum.TryParse(command.GameDataType, out PragmaticPlayTransferGameDataType gameDataType))
        {
            return;
        }
        var key = $"GameRecord:PragmaticPlayService:{command.GameDataType}".ToLower();
        if (!await _scheduleDispatch.ExistAsync(key: key).ConfigureAwait(false))
        {
            if (await _scheduleDispatch.LockingAsync(key: key, command: command).ConfigureAwait(false))
            {
                try
                {
                    var nowTime = DateTimeOffset.ParseExact(_systemClock.UtcNow.ToString(@"yyyy-MM-dd\THH:mm:00K"), @"yyyy-MM-dd\THH:mm:ssK", null, styles: System.Globalization.DateTimeStyles.RoundtripKind);
                    var timeFrom = nowTime.AddMinutes((double)_gameRecordInfoOptions.IntervalMinute * -1);

                    var result = await _gameRecordQueryService
                            .GetAsync(new GameRecordQuery(gameDataType, timeFrom), cancellationToken)
                            .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "發生例外");
                }
                finally
                {
                    await _scheduleDispatch.UnlockAsync(key: key).ConfigureAwait(false);
                }
            }
        }
    }
}
