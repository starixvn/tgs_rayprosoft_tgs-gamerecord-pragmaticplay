using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameCommand.Client;
using Tgs.GameCommand.Common;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlay.Core.JobApplicationServices;

internal class JobInitCommandService(
	ILogger<JobInitCommandService> logger,
	IOptions<GameRecordInfoOptions> options,
	IHangfireLockRepository hangfireLock,
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

			var gameProviderConfigs = await client.GameProviderConfigurationQueryAsync(
				gameProviderCode: _gameRecordInfo.GameProviderCode,
				cancellationToken: cancellationToken)
				.ToArrayAsync(cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? Enumerable.Empty<GameProviderConfigInfo>();

			if (gameProviderConfigs == null || !gameProviderConfigs.Any())
			{
				return;
			}

			var gameProviderAgentMapper = new Dictionary<string, Guid>();

			foreach (var gameProviderConfig in gameProviderConfigs)
			{
				if (string.IsNullOrWhiteSpace(gameProviderConfig.ConfigurationData) || gameProviderConfig.Status == GameProviderConfigStatus.Inactive)
				{
					continue;
				}

				var configurationDataJson = JsonNode.Parse(gameProviderConfig.ConfigurationData)
					?? throw new Exception($"Game Provider Configuration does not exist.({gameProviderConfig.GameProviderConfigId})");

				var apiUrl = new Uri(configurationDataJson["ApiUrl"]?.ToString()
					?? settingConfigurationJson["ApiUrl"]?.ToString()
					?? throw new FormatException($"Required parameters(ApiUrl)"));
				var secretKey = configurationDataJson["SecretKey"]?.ToString()
					?? settingConfigurationJson["SecretKey"]?.ToString()
					?? throw new FormatException($"Required parameters(SecretKey)");
				var secureLogin = configurationDataJson["SecureLogin"]?.ToString()
					?? settingConfigurationJson["SecureLogin"]?.ToString()
					?? throw new FormatException($"Required parameters(SecureLogin)");

				if (gameProviderAgentMapper.ContainsKey(secureLogin))
				{
					continue;
				}

				gameProviderAgentMapper.Add(secureLogin, gameProviderConfig.GameProviderConfigId);

				foreach (var gameDataType in _gameRecordInfo.GameDataTypes)
				{
					var recurringJobId = string.Concat(secureLogin, '_', gameDataType, '|', gameProviderConfig.GameProviderConfigId);
					var jobExecuteCommand = new JobExecuteCommand(
						ApiUrl: apiUrl,
						SecureLogin: secureLogin,
						SecretKey: secretKey,
						DataType: gameDataType);

					RecurringJob.AddOrUpdate<IHangfireUtensilRepository>(
						recurringJobId: recurringJobId,
						service => service.JobExecuteAsync(jobExecuteCommand),
						cronExpression: Cron.Minutely(),
						options: _recurringJobOptions);

					//初始化強制第一次處理
					_ = BackgroundJob.Enqueue<IHangfireUtensilRepository>(service => service.JobExecuteAsync(jobExecuteCommand));
				}
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "LogOn:{logOn} Message:遊戲廠商初始化排程發生例外", DateTime.Now.ToString("o"));
		}
		finally
		{
			_ = BackgroundJob.Schedule<IHangfireUtensilRepository>(service => service.JobCheckAsync(), TimeSpan.FromMinutes(1));
		}
	}
}
