using MongoDB.Driver;
using StackExchange.Redis;
using Tgs.GameRecord.PragmaticPlay.Core;
using Tgs.GameRecord.PragmaticPlay.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGameRecordRepositories(
		this IServiceCollection services,
		Func<IServiceProvider, IMongoClient> mongoClientResolver,
		Func<IServiceProvider, IDatabase> redisDatabaseResolver)
		=> services
		.AddSingleton<IUtensilRepository, UtensilRepository>()
		.AddTransient<IHangfireUtensilRepository, HangfireUtensilRepository>()
		.AddTransient<IHangfireLockRepository>(sp => ActivatorUtilities.CreateInstance<HangfireLockRepository>(sp, redisDatabaseResolver(sp)))
		.AddTransient<IGameRecordScheduleRepository>((sp) => ActivatorUtilities.CreateInstance<GameRecordScheduleRepository>(sp, mongoClientResolver(sp)))
		.AddSingleton<IGameRecordScheduleLockRepository>(sp => ActivatorUtilities.CreateInstance<GameRecordScheduleLockRepository>(sp, redisDatabaseResolver(sp)));
}
