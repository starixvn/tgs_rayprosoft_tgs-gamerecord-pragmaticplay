using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameRecord.PragmaticPlay.Common;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.PragmaticPlay.Core.JobApplicationServices;

internal class JobExecuteCommandService(
	ILogger<JobExecuteCommandService> logger,
	IMessageSender messageSender) : ICommandService<JobExecuteCommand>
{
	public ValueTask ExecuteAsync(JobExecuteCommand command, CancellationToken cancellationToken = default)
	{
		var message = new PragmaticPlayGameRecordScheduleMessage
		{
			ApiUrl = command.ApiUrl,
			SecureLogin = command.SecureLogin,
			SecretKey = command.SecretKey,
			DataType = command.DataType
		};

		logger.LogInformation("{logInformation}",
			JsonSerializer.Serialize(new
			{
				LogOn = DateTime.Now.ToString("u"),
				Subject = PragmaticPlayNatsConst.SchedulePublish,
				Data = new
				{
					message.ApiUrl,
					message.SecureLogin,
					message.SecretKey,
					message.DataType
				},
				Message = "This message publishing handler."
			}));

		return messageSender.PublishAsync(
			subject: PragmaticPlayNatsConst.SchedulePublish,
			data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)),
			cancellationToken: cancellationToken);
	}
}
