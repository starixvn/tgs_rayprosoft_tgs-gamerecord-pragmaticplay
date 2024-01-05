using Microsoft.Extensions.Logging;
using Tgs.GameInterface.PragmaticPlay.Client;
using Tgs.GameInterface.PragmaticPlay.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlay.Core.ApplicationServices;

internal class ManualPullRecordCommandService(
    ILogger<ManualPullRecordCommandService> logger,
    IGameInterfacePragmaticPlayClient client,
    IUtensil utensil,
    ISystemClock systemClock,
    ICommandService<HandleRecordCommand> service) : ICommandService<ManualPullRecordCommand>
{
    public async ValueTask ExecuteAsync(ManualPullRecordCommand command, CancellationToken cancellationToken = default)
    {
        var nextTime = command.StartTime;
        var endTime = command.EndTime;
        var currentTime = systemClock.CurrentTime;

        while (utensil.IsRun(nextTime, endTime, currentTime))
        {
            var result = command.DataType.ToUpper() switch
            {
                "LC" => await client
                .GameRecordByLiveAsync(
                    apiUrl: command.ApiUrl,
                    secureLogin: command.SecureLogin,
                    secretKey: command.SecretKey,
                    startTime: nextTime,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false),
                "VSB" => await client
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

            if (result.NextTime.ToDateTimeOffset() == DateTimeOffset.UnixEpoch)
            {
                break;
            }
            else
            {
                nextTime = result.NextTime.ToDateTimeOffset();
            }

            logger.LogDebug("DataType:{dataType} | NextTime:{nextTime} | endTime:{endTime} | currentTime:{currentTime}| while{isRun}", command.DataType.ToUpper(), nextTime.ToString("O"), currentTime.ToString("O"), currentTime.ToString("O"), utensil.IsRun(nextTime, currentTime, currentTime));

            var gameRecords = result.Items
                ?? Enumerable.Empty<GameRecordData>();

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
    }
}
