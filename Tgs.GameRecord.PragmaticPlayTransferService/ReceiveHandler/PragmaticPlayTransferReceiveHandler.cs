using System.Text.Json;
using Tgs.GameProviders.Domains;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.ReceiveHandler;

public class PragmaticPlayTransferReceiveHandler : IMessageHandler
{
    private readonly ICommandService<PragmaticPlayTransferJobWorkerCommand> _commandService;
    private readonly ILogger<PragmaticPlayTransferReceiveHandler> _logger;

    public PragmaticPlayTransferReceiveHandler(
        ICommandService<PragmaticPlayTransferJobWorkerCommand> commandService,
        ILogger<PragmaticPlayTransferReceiveHandler> logger)
    {
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
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

        return _commandService.ExecuteAsync(new PragmaticPlayTransferJobWorkerCommand(), cancellationToken);
    }
}
