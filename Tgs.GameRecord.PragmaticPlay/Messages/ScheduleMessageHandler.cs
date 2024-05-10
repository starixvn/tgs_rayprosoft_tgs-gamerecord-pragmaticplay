using System.Text;
using System.Text.Json;
using Tgs.GameRecord.PragmaticPlay.Common;
using Tgs.GameRecord.PragmaticPlay.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.PragmaticPlay.Messages;

internal class ScheduleMessageHandler(
    ILogger<ScheduleMessageHandler> logger,
    ICommandService<SchedulePullRecordCommand> service) : IMessageHandler<ReadOnlyMemory<byte>>
{
    public ValueTask HandleAsync(string subject, ReadOnlyMemory<byte> data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);
        var message = JsonSerializer.Deserialize<PragmaticPlayGameRecordScheduleMessage>(json)
            ?? throw new NullReferenceException("The request data resolves to a null value");

		logger.LogInformation("LogOn:{logOn} Subject:{subject} Information Request:{message}",
			DateTime.Now.ToString("o"),
			subject,
			message);

		var command = new SchedulePullRecordCommand(
            ApiUrl: message.ApiUrl,
            SecureLogin: message.SecureLogin,
            SecretKey: message.SecretKey,
            DataType: message.DataType);

        return service.ExecuteAsync(
            command: command,
            cancellationToken: cancellationToken);
    }
}
