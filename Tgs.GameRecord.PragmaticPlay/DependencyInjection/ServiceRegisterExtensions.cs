using Tgs.GameInterface.PragmaticPlay.Client;
using Tgs.GameRecord.PragmaticPlay.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceRegisterExtensions
{
	public static async Task<WebApplication> UseHangfireRegisterAsync(this WebApplication app)
	{
		using (var scope = app.Services.CreateScope())
		{
			var sp = scope.ServiceProvider;
			var hangfireSchedule = scope.ServiceProvider.GetRequiredService<IHangfireUtensilRepository>();
			await hangfireSchedule.JobInitAsync().ConfigureAwait(false);
		}

		return app;
	}

	public static async Task<WebApplication> UseRegisterAsync(this WebApplication app)
	{
		using (var scope = app.Services.CreateScope())
		{
			var gameRecordInfo = app.Configuration.GetSection("GameRecordInfo").Get<GameRecordInfoOptions>()!;
			var client = scope.ServiceProvider.GetRequiredService<IGameInterfacePragmaticPlayClient>();
			_ = await client
				.CheckingServiceAsync(
				machineName: Environment.MachineName)
				.ConfigureAwait(false);
		}

		return app;
	}
}
