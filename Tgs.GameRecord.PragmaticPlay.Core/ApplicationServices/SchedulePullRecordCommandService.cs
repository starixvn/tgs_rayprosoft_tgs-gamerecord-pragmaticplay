using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameInterface.PragmaticPlay.Client;
using Tgs.GameInterface.PragmaticPlay.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlay.Core.ApplicationServices;

internal class SchedulePullRecordCommandService(
    ILogger<SchedulePullRecordCommandService> logger,
    IOptions<GameRecordInfoOptions> options,
    ISchedulePullRecordLock schedulePullRecordLock,
    IUtensil utensil,
    IGameRecordScheduleService gameRecordScheduleService,
    ISystemClock systemClock,
    IGameInterfacePragmaticPlayClient client,
    ICommandService<HandleRecordCommand> service) : ICommandService<SchedulePullRecordCommand>
{
    private readonly GameRecordInfoOptions _gameRecordInfoOptions = options.Value;

    public async ValueTask ExecuteAsync(SchedulePullRecordCommand command, CancellationToken cancellationToken = default)
    {
        if (!await schedulePullRecordLock
            .LockingAsync(
                command.DataType)
            .ConfigureAwait(false))
        {
            logger.LogWarning("LogOn:{logOn} Message:This data type ({DataType}) pull record has been locked.",
                DateTime.Now.ToString("o"),
                command.DataType);
            return;
        }

        try
        {
            var gameProviderId = await utensil
                .GetGameProviderIdAsync(
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            var gameRecordSchedule =
                await gameRecordScheduleService
                .GetAsync(
                    dataType: command.DataType,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false) ??
                await gameRecordScheduleService
                .InitAsync(
                    gameProviderId: gameProviderId,
                    dataType: command.DataType,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false)
                ?? throw new Exception($"Failed to get Game Record Schedule data.({command.DataType})");

            var config = gameRecordScheduleService.GetConfig(gameRecordSchedule);

            var startTime = command.DataType switch
            {
                var dataType when dataType.Equals("LC", StringComparison.OrdinalIgnoreCase)
                => config.NextTime.AddMinutes(-_gameRecordInfoOptions.LiveDealerIntervalMinutes),
                var dataType when dataType.Equals("VSB", StringComparison.OrdinalIgnoreCase)
                => config.NextTime.AddMinutes(-_gameRecordInfoOptions.VirtualSportsIntervalMinutes),
                _ => config.NextTime.AddMinutes(-_gameRecordInfoOptions.IntervalMinutes)
            };

            var nextTime = startTime;
            var currentTime = systemClock.CurrentTime;

            while (utensil.IsRun(nextTime, currentTime, currentTime))
            {
                var result = command.DataType.ToUpper() switch
                {
                    var dataType when dataType.Equals("LC", StringComparison.OrdinalIgnoreCase)
                    => await client
                    .GameRecordByLiveAsync(
                        apiUrl: command.ApiUrl,
                        secureLogin: command.SecureLogin,
                        secretKey: command.SecretKey,
                        startTime: nextTime,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false),
                    var dataType when dataType.Equals("VSB", StringComparison.OrdinalIgnoreCase)
                    => await client
                    .GameRecordByVirtualSportsAsync(
                        apiUrl: command.ApiUrl,
                        secureLogin: command.SecureLogin,
                        secretKey: command.SecretKey,
                        startTime: nextTime,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false),
                    _ => await client
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

                logger.LogDebug("DataType:{dataType} | NextTime:{nextTime} | endTime:{endTime} | currentTime:{currentTime}| while{isRun}", command.DataType.ToUpper(), nextTime.ToString("O"), currentTime.ToString("O"), currentTime.ToString("O"), utensil.IsRun(nextTime, currentTime, currentTime));

                if (!gameRecords.Any())
                {
                    continue;
                }

                foreach (var gameRecord in gameRecords)
                {
                    logger.LogDebug("LogOn:{logOn} Message: Settlement. GameRecords:{gameRecord}", DateTime.Now.ToString("o"), gameRecord);

                    try
                    {
                        await service
                            .ExecuteAsync(
                                command: new(gameRecord),
                                cancellationToken: cancellationToken)
                            .ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "LogOn:{logOn} Message: An exception occurred in this game record.({gameRecord})", DateTime.Now.ToString("o"), gameRecord);
                    }
                }
            }
            _ = await gameRecordScheduleService.UpdateAsync(
                gameRecordSchedule: gameRecordSchedule,
                 dataType: command.DataType,
                 nextTime: nextTime,
                 cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "LogOn:{logOn} Message: Exception.", DateTime.Now.ToString("o"));
        }
        finally
        {
            _ = await schedulePullRecordLock
                .UnlockAsync(
                    command.DataType)
                .ConfigureAwait(false);
        }
    }
}
