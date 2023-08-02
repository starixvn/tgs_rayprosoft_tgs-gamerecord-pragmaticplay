using Microsoft.Extensions.Logging;
using Tgs.GameRounds.Schedule.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlay.Core.JobApplicationServices;

internal class JobExecuteCommandService : ICommandService<JobExecuteCommand>
{
    private readonly IGameRoundsScheduleClient _client;
    private readonly ILogger<JobExecuteCommandService> _logger;

    public JobExecuteCommandService(
        ILogger<JobExecuteCommandService> logger,
        IGameRoundsScheduleClient client)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public ValueTask ExecuteAsync(JobExecuteCommand command, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            command.GameProviderCode,
            command.ApiUrl,
            command.SecureLogin,
            command.SecretKey,
            command.DataType
        };

        _logger.LogInformation("LogOn:{logOn} Parameters:{parameters}", DateTime.Now.ToString("u"), parameters);

        return _client
            .PragmaticPlay_ScheduleFormQueuedAsync(
            gameProviderCode: command.GameProviderCode,
            name: command.Name,
            subject: "gp.gamerecord.scheduleform.pptf.queued",
            apiUrl: parameters.ApiUrl,
            secureLogin: parameters.SecureLogin,
            secretKey: parameters.SecretKey,
            dataType: parameters.DataType,
            cancellationToken: cancellationToken);
    }
}
