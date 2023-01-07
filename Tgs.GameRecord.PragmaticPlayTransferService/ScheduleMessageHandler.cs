using System.Text.Json;
using Tgs.GameProviders.Domains;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.DispatchServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService;

public class ScheduleMessageHandler : IMessageHandler
{
    private readonly ICommandService<ScheduleDispatchCommand> _commandService;
    private readonly ILogger<ScheduleMessageHandler> _logger;

    public ScheduleMessageHandler(
        ICommandService<ScheduleDispatchCommand> commandService,
        ILogger<ScheduleMessageHandler> logger)
    {
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var command = JsonSerializer.Deserialize<ScheduleMessageModel>(data.Span);

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

        var scheduleDispatchCommand = new ScheduleDispatchCommand(
            command.GameProviderId,
            command.GameProviderCode,
            command.GameDataType);

        return _commandService.ExecuteAsync(
            command: scheduleDispatchCommand,
            cancellationToken: cancellationToken);
    }
}
