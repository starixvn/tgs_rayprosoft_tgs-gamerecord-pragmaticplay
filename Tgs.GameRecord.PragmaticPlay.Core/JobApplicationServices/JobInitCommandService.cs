using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameCommand.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlay.Core.JobApplicationServices;

internal class JobInitCommandService : ICommandService<JobInitCommand>
{
    private readonly IGameCommandClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly ILogger<JobInitCommandService> _logger;
    private readonly RecurringJobOptions _recurringJobOptions;

    public JobInitCommandService(
        ILogger<JobInitCommandService> logger,
        IOptions<GameRecordInfoOptions> options,
        IGameCommandClient client)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameRecordInfo = options.Value;
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _recurringJobOptions = new RecurringJobOptions()
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "China Standard Time" : "Asia/Shanghai")
        };
    }

    public async ValueTask ExecuteAsync(JobInitCommand command, CancellationToken cancellationToken = default)
    {
        using (var connection = JobStorage.Current.GetConnection())
        {
            foreach (var recurringJob in connection.GetRecurringJobs())
            {
                RecurringJob.RemoveIfExists(recurringJob.Id);
            }
        }

        try
        {
            var settingConfiguration = await _client
                .SettingGetConfigurationStringAsync(
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
            _logger.LogError(ex, "LogOn:{logOn} Message:遊戲廠商初始化排程發生例外", DateTime.Now.ToString("o"));
        }
        finally
        {
            _ = BackgroundJob.Schedule<IHangfireUtensil>(service => service.JobCheckAsync(), TimeSpan.FromMinutes(1));
        }
    }
}
