using Tgs.GameCommand.Client;
using Tgs.GameInterface.PragmaticPlay.Client;
using Tgs.GameRecord.PragmaticPlay;
using Tgs.GameRecord.PragmaticPlay.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGameRecord(
		this IServiceCollection services,
		Action<MongoDBOptions, IServiceProvider> mongoDBconfigure,
		Action<RedisOptions, IServiceProvider> redisDBconfigure,
		Action<GameRecordInfoOptions, IServiceProvider> gameRecordInfoOptions,
		Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
		Action<GameInterfacePragmaticPlayClientOptions, IServiceProvider> gameInterfacePragmaticPlayClientOptions)
		=> services
		.AddOptions<RedisOptions>()
		.Configure(redisDBconfigure)
		.Services
		.AddOptions<MongoDBOptions>()
		.Configure(mongoDBconfigure)
		.Services
		.AddOptions<GameRecordInfoOptions>()
		.Configure(gameRecordInfoOptions)
		.Services
		.AddSingleton<RedisConnectionManager>()
		.AddSingleton<MongoDBConnectionManager>()
		.AddSingleton(TimeProvider.System)
		.AddGameRecordCore(
			gameCommandClientOptions: gameCommandClientOptions,
			gameInterfacePragmaticPlayClientOptions: gameInterfacePragmaticPlayClientOptions)
		.AddGameRecordRepositories(
			mongoClientResolver: (sp) => sp.GetRequiredService<MongoDBConnectionManager>().MongoClient,
			redisDatabaseResolver: (sp) => sp.GetRequiredService<RedisConnectionManager>().Database);
}
