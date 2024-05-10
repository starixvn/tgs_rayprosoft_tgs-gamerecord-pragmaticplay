using System.Reflection;
using StackExchange.Redis;
using Tgs.GameCommand.Client;
using Tgs.GameInterface.PragmaticPlay.Client;
using Tgs.GameRecord.PragmaticPlay.Core;
using Tgs.GameRecordSchedules.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGameRecordCore(
		this IServiceCollection services,
		Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
		Action<GameInterfacePragmaticPlayClientOptions, IServiceProvider> gameInterfacePragmaticPlayClientOptions,
		Action<GameRecordSchedulesClientOptions, IServiceProvider> gameRecordSchedulesClientOptions,
		Func<IServiceProvider, IDatabase> redisDatabaseResolver)
		=> services
		.RegisterAllServiceUtilityTypes(
			Assembly.GetExecutingAssembly(),
			ServiceLifetime.Transient)
		.AddMemoryCache()
		.AddGameCommandClient(gameCommandClientOptions)
		.AddGameInterfacePragmaticPlayClient(gameInterfacePragmaticPlayClientOptions)
		.AddGameRecordSchedulesClient(gameRecordSchedulesClientOptions)
		.AddSingleton(TimeProvider.System)
		.AddSingleton<ISchedulePullRecordLock>(sp => ActivatorUtilities.CreateInstance<SchedulePullRecordLock>(sp, redisDatabaseResolver(sp)))
		.AddSingleton<IUtensil, UtensilRepository>()
		.AddTransient<IGameRecordScheduleService, GameRecordScheduleService>()
		.AddTransient<IHangfireUtensil, HangfireUtensil>()
		.AddTransient<IHangfireLock>(sp => ActivatorUtilities.CreateInstance<HangfireLock>(sp, redisDatabaseResolver(sp)));
}
