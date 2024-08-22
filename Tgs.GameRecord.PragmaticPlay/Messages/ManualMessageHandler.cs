using System.Text;
using System.Text.Json;
using Tgs.GameRecord.PragmaticPlay.Common;
using Tgs.GameRecord.PragmaticPlay.Core;
using Tgs.GameRecord.PragmaticPlay.Core.ApplicationServices;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.PragmaticPlay.Messages;

internal class ManualMessageHandler(
	ILogger<ManualMessageHandler> logger,
	IManualPullRecordDispatcher dispatcher) : IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(string subject, ReadOnlyMemory<byte> data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		var json = Encoding.UTF8.GetString(data.Span);
		var message = JsonSerializer.Deserialize<PragmaticPlayGameRecordManualMessage>(json)
			?? throw new NullReferenceException("The request data resolves to a null value");

		logger.LogInformation("{logInformation}",
			JsonSerializer.Serialize(new
			{
				LogOn = DateTime.Now.ToString("u"),
				Subject = subject,
				Data = new
				{
					message.ApiUrl,
					message.SecureLogin,
					message.SecretKey,
					message.DataType,
					StartTime = message.StartTime.ToString("u"),
					EndTime = message.EndTime.ToString("u"),
				},
				Message = "This message receiving handler."
			}));

		return dispatcher.PushCommandAsync(
			command: new ManualPullRecordCommand(
				ApiUrl: message.ApiUrl,
				SecureLogin: message.SecureLogin,
				SecretKey: message.SecretKey,
				DataType: message.DataType,
				StartTime: message.StartTime,
				EndTime: message.EndTime),
			cancellationToken: cancellationToken);
	}
}
