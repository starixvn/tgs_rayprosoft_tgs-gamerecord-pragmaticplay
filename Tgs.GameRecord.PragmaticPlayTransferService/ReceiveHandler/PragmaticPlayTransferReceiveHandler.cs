using System.Text.Json;
using Tgs.GameProviders.Domains;
using Tgs.MessageQueue;

namespace Tgs.GameRecord.PragmaticPlayTransferService.ReceiveHandler;

public class PragmaticPlayTransferReceiveHandler : IMessageHandler
{
    private readonly ILogger<PragmaticPlayTransferReceiveHandler> _logger;

    public PragmaticPlayTransferReceiveHandler(
        ILogger<PragmaticPlayTransferReceiveHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var command = JsonSerializer.Deserialize<PragmaticPlayTransferCommand>(data.Span);

        var logInfo = new
        {
            GameProviderCode = GameProviderType.PragmaticPlayTransfer,
            Data = command,
            ReceiveTime = DateTimeOffset.Now.ToString("O")
        };

        if (command == null)
        {
            _logger.LogError("Information Received:{LogInfo}", logInfo);
            return ValueTask.CompletedTask;
        }

        _logger.LogInformation("Information Received:{LogInfo}", logInfo);

        return ValueTask.CompletedTask;
    }
}
