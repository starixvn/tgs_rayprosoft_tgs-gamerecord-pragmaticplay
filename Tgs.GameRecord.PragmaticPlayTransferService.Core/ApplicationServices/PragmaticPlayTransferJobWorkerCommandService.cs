using Microsoft.Extensions.Logging;
using Tgs.GameProviders.Client;
using Tgs.GameProviders.Domains;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;

internal class PragmaticPlayTransferJobWorkerCommandService : ICommandService<PragmaticPlayTransferJobWorkerCommand>
{
    private readonly IGameProviderService _gameProviderService;
    private readonly IGetService<GameRecordQuery, GameRecordResult> _gameRecordQueryService;
    private readonly ILogger<PragmaticPlayTransferJobWorkerCommandService> _logger;
    private readonly IGameRecordScheduleService _scheduleService;

    public PragmaticPlayTransferJobWorkerCommandService(
        IGameProviderService gameProviderService,
        IGameRecordScheduleService scheduleService,
        IGetService<GameRecordQuery, GameRecordResult> gameRecordQueryService,
        ILogger<PragmaticPlayTransferJobWorkerCommandService> logger)
    {
        _gameProviderService = gameProviderService ?? throw new ArgumentNullException(nameof(gameProviderService));
        _scheduleService = scheduleService ?? throw new ArgumentNullException(nameof(scheduleService));
        _gameRecordQueryService = gameRecordQueryService ?? throw new ArgumentNullException(nameof(gameRecordQueryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask ExecuteAsync(
        PragmaticPlayTransferJobWorkerCommand command,
        CancellationToken cancellationToken = default)
    {
        var gameDataTypes = Enum.GetValues(typeof(PragmaticPlayTransferGameDataType)).Cast<PragmaticPlayTransferGameDataType>();

        var gameProviderId = await _gameProviderService
            .GetGameProviderIdAsync(GameProviderType.PragmaticPlayTransfer)
            .ConfigureAwait(false);

        foreach (var gameDataType in gameDataTypes)
        {
            try
            {
                var schedule = await _scheduleService
                                   .GetAsync(gameDataType, cancellationToken)
                                   .ConfigureAwait(false)
                               ?? await _scheduleService
                                   .InitAsync(gameProviderId, gameDataType, cancellationToken)
                                   .ConfigureAwait(false);

                var sysTime = DateTimeOffset.UtcNow;

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
