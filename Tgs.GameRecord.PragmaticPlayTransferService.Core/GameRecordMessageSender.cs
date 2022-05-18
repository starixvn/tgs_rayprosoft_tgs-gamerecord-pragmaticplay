using System.Text.Json;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Tgs.MessageQueue;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

internal class GameRecordMessageSender : IGameRecordMessageSender
{
    private readonly ILogger<GameRecordMessageSender> _logger;
    private readonly IMessageSender _messageSender;

    public GameRecordMessageSender(
        IMessageSender messageSender,
        ILogger<GameRecordMessageSender> logger)
    {
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ValueTask SendGameRoundAsync(GameRoundData data)
    {
        _logger.Log(LogLevel.Information,
            new EventId(1000, nameof(GameRecordMessageSender)),
            new
            {
                MessageQueue = "tgs.gp.gamerecord.database.round",
                Data = data,
                SendDateTime = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(+8)).ToString("O")
            },
            null,
            (state, ex) => JsonSerializer.Serialize(state));

        return _messageSender.PublishAsync(
            $"tgs.gp.gamerecord.database.round.{data.GameProviderCode.ToLower()}",
            data.ToBson());
    }
}
