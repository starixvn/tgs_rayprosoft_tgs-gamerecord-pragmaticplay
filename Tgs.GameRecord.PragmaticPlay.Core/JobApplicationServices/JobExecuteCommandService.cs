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
        var parameters = new PragmaticPlayGameRecordScheduleMessage
        {
            ApiUrl = command.ApiUrl,
            SecureLogin = command.SecureLogin,
            SecretKey = command.SecretKey,
            DataType = command.DataType
        };

        logger.LogInformation("LogOn:{logOn} Parameters:{parameters}", DateTime.Now.ToString("u"), parameters);

        return messageSender.PublishAsync(
            subject: PragmaticPlayNatsConst.SchedulePublish,
            data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(parameters)),
            cancellationToken: cancellationToken);
    }
}
