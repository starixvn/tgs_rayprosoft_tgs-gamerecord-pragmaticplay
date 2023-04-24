using Microsoft.Extensions.Logging;
using Tgs.GameInterface.PragmaticPlay.Client;
using Tgs.GameInterface.PragmaticPlay.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;

internal class ManualPullRecordCommandService : ICommandService<ManualPullRecordCommand>
{
    private readonly IGameInterfacePragmaticPlayClient _client;
    private readonly IGameRecordUtensil _gameRecordUtensil;
    private readonly ILogger<ManualPullRecordCommandService> _logger;
    private readonly ICommandService<ManualHandleRecordCommand> _service;

    public ManualPullRecordCommandService(
        ILogger<ManualPullRecordCommandService> logger,
        IGameInterfacePragmaticPlayClient client,
        IGameRecordUtensil gameRecordUtensil,
        ICommandService<ManualHandleRecordCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _gameRecordUtensil = gameRecordUtensil ?? throw new ArgumentNullException(nameof(gameRecordUtensil));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(ManualPullRecordCommand command, CancellationToken cancellationToken = default)
    {
        var nextTime = command.StartTime;
        var endTime = command.EndTime;

        while (nextTime < endTime && nextTime < endTime && endTime.Subtract(nextTime).TotalSeconds > 30)
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

            nextTime = result.NextTime.ToDateTimeOffset();

            var gameRecords = result.Items
                ?? Enumerable.Empty<GameRecordData>();

            if (nextTime == DateTimeOffset.UnixEpoch || !gameRecords.Any())
            {
                break;
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
    }
}
