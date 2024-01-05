using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameCommand.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlay.Core.JobApplicationServices;

internal class JobInitCommandService(
    ILogger<JobInitCommandService> logger,
    IOptions<GameRecordInfoOptions> options,
    IHangfireLock hangfireLock,
    IGameCommandClient client) : ICommandService<JobInitCommand>
{
    private readonly GameRecordInfoOptions _gameRecordInfo = options.Value;

    private readonly RecurringJobOptions _recurringJobOptions = new()
    {
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "China Standard Time" : "Asia/Shanghai")
    };

    public async ValueTask ExecuteAsync(JobInitCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await hangfireLock.LockingAsync(
                application: typeof(JobInitCommandService).Name,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false))
            {
                logger.LogWarning("LogOn:{logOn} Message:進行初始化動作，已經有其他服務進行", DateTime.Now.ToString("o"));
                return;
            }

            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (var recurringJob in connection.GetRecurringJobs())
                {
                    RecurringJob.RemoveIfExists(recurringJob.Id);
                }
            }

            var settingConfiguration = await client.SettingGetConfigurationStringAsync(
                key: _gameRecordInfo.GameProviderConfiguration,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false)
                ?? throw new Exception($"This game product setting does not exist.({_gameRecordInfo.GameProviderConfiguration})");

            var settingConfigurationJson = JsonNode.Parse(settingConfiguration)
                ?? throw new Exception($"This game product setting does not exist.({_gameRecordInfo.GameProviderConfiguration})");

            foreach (var gameDataType in _gameRecordInfo.GameDataTypes)
            {
                var apiUrl = new Uri(settingConfigurationJson["ApiUrl"]?.ToString()
                    ?? throw new Exception($"Required parameters(ApiUrl)"));
                var secretKey = settingConfigurationJson["SecretKey"]?.ToString()
                    ?? throw new Exception($"Required parameters(SecretKey)");
                var secureLogin = settingConfigurationJson["SecureLogin"]?.ToString()
                    ?? throw new Exception($"Required parameters(SecureLogin)");

                var jobExecuteCommand = new JobExecuteCommand(
                    GameProviderCode: _gameRecordInfo.GameProviderCode,
                    Name: $"{_gameRecordInfo.GameProviderCode}-{gameDataType}",
                    ApiUrl: apiUrl,
                    SecureLogin: secureLogin,
                    SecretKey: secretKey,
                    DataType: gameDataType);

                RecurringJob.AddOrUpdate<IHangfireUtensil>(
                    recurringJobId: jobExecuteCommand.Name,
                    service => service.JobExecuteAsync(jobExecuteCommand),
                    cronExpression: Cron.Minutely(),
                    options: _recurringJobOptions);

                //初始化強制第一次處理
                _ = BackgroundJob.Enqueue<IHangfireUtensil>(service => service.JobExecuteAsync(jobExecuteCommand));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "LogOn:{logOn} Message:遊戲廠商初始化排程發生例外", DateTime.Now.ToString("o"));
        }
        finally
        {
            _ = BackgroundJob.Schedule<IHangfireUtensil>(service => service.JobCheckAsync(), TimeSpan.FromMinutes(1));
        }
    }
}
