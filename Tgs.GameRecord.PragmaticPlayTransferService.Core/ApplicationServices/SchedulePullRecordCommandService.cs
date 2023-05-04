using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameInterface.PragmaticPlay.Client;
using Tgs.GameInterface.PragmaticPlay.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;

internal class SchedulePullRecordCommandService : ICommandService<SchedulePullRecordCommand>
{
    private readonly IGameInterfacePragmaticPlayClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfoOptions;
    private readonly IGameRecordScheduleService _gameRecordScheduleService;
    private readonly IGameRecordUtensil _gameRecordUtensil;
    private readonly ILogger<SchedulePullRecordCommandService> _logger;
    private readonly ISchedulePullRecordLock _schedulePullRecordLock;
    private readonly ICommandService<ScheduleHandleRecordCommand> _service;
    private readonly ISystemClock _systemClock;
    private readonly IUtensil _utensil;

    public SchedulePullRecordCommandService(
        ILogger<SchedulePullRecordCommandService> logger,
        IOptions<GameRecordInfoOptions> options,
        ISchedulePullRecordLock schedulePullRecordLock,
        IUtensil utensil,
        IGameRecordScheduleService gameRecordScheduleService,
        ISystemClock systemClock,
        IGameInterfacePragmaticPlayClient client,
        IGameRecordUtensil gameRecordUtensil,
        ICommandService<ScheduleHandleRecordCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameRecordInfoOptions = options.Value;
        _schedulePullRecordLock = schedulePullRecordLock ?? throw new ArgumentNullException(nameof(schedulePullRecordLock));
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
        _gameRecordScheduleService = gameRecordScheduleService ?? throw new ArgumentNullException(nameof(gameRecordScheduleService));
        _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _gameRecordUtensil = gameRecordUtensil ?? throw new ArgumentNullException(nameof(gameRecordUtensil));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(SchedulePullRecordCommand command, CancellationToken cancellationToken = default)
    {
        if (!await _schedulePullRecordLock
            .LockingAsync(
                command.DataType)
            .ConfigureAwait(false))
        {
            _logger.LogWarning("LogOn:{logOn} Message:This data type ({DataType}) pull record has been locked.",
                DateTime.Now.ToString("o"),
                command.DataType);
            return;
        }

        try
        {
            var gameProviderId = await _utensil
                .GetGameProviderIdAsync(
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            var gameRecordSchedule =
                await _gameRecordScheduleService
                .GetAsync(
                    dataType: command.DataType,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false) ??
                await _gameRecordScheduleService
                .InitAsync(
                    gameProviderId: gameProviderId,
                    dataType: command.DataType,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false)
                ?? throw new Exception($"Failed to get Game Record Schedule data.({command.DataType})");

            var config = _gameRecordScheduleService.GetConfig(gameRecordSchedule);

            var startTime = command.DataType.ToUpper() switch
            {
                "LC" => config.NextTime.AddMinutes(-_gameRecordInfoOptions.LiveDealerIntervalMinutes),
                "VSB" => config.NextTime.AddMinutes(-_gameRecordInfoOptions.VirtualSportsIntervalMinutes),
                _ => config.NextTime
            };

            var nextTime = startTime;
            var currentTime = _systemClock.CurrentTime;

            while (_utensil.IsRun(nextTime, currentTime, currentTime))
            {
                var result = command.DataType.ToUpper() switch
                {
                    "LC" => await _client
                    .GameRecordByLiveAsync(
                        apiUrl: command.ApiUrl,
                        secureLogin: command.SecureLogin,
                        secretKey: command.SecretKey,
                        startTime: nextTime,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false),
                    "VSB" => await _client
                    .GameRecordByVirtualSportsAsync(
                        apiUrl: command.ApiUrl,
                        secureLogin: command.SecureLogin,
                        secretKey: command.SecretKey,
                        startTime: nextTime,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false),
                    _ => await _client
                    .GameRecordBySlotAsync(
                        apiUrl: command.ApiUrl,
                        secureLogin: command.SecureLogin,
                        secretKey: command.SecretKey,
                        startTime: nextTime,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false)
                };

                var gameRecords = result.Items
                    ?? Enumerable.Empty<GameRecordData>();

                if (result.NextTime.ToDateTimeOffset() == DateTimeOffset.UnixEpoch)
                {
                    break;
                }
                else
                {
                    nextTime = result.NextTime.ToDateTimeOffset();
                }

                _logger.LogDebug("DataType:{dataType} | NextTime:{nextTime} | endTime:{endTime} | currentTime:{currentTime}| while{isRun}", command.DataType.ToUpper(), nextTime.ToString("O"), currentTime.ToString("O"), currentTime.ToString("O"), _utensil.IsRun(nextTime, currentTime, currentTime));

                if (!gameRecords.Any())
                {
                    continue;
                }

                foreach (var gameRecord in gameRecords)
                {
                    _logger.LogDebug("LogOn:{logOn} Message: Settlement. GameRecords:{gameRecord}", DateTime.Now.ToString("o"), gameRecord);

                    try
                    {
                        var record = await _gameRecordUtensil
                            .SettlementConvertRecordAsync(
                                gameRecord,
                                cancellationToken)
                            .ConfigureAwait(false);

                        _logger.LogDebug("LogOn:{logOn} Message: Settlement. Record:{record}", DateTime.Now.ToString("o"), record);

                        await _service
                            .ExecuteAsync(
                                command: new(record),
                                cancellationToken: cancellationToken)
                            .ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "LogOn:{logOn} Message: Settlement Exception. GameRecord:{gameRecord}", DateTime.Now.ToString("o"), gameRecord);
                    }
                }
            }
            _ = await _gameRecordScheduleService.UpdateAsync(
                gameRecordSchedule: gameRecordSchedule,
                 dataType: command.DataType,
                 nextTime: nextTime,
                 cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LogOn:{logOn} Message: Exception.", DateTime.Now.ToString("o"));
        }
        finally
        {
            _ = await _schedulePullRecordLock
                .UnlockAsync(
                    command.DataType)
                .ConfigureAwait(false);
        }
    }
}
