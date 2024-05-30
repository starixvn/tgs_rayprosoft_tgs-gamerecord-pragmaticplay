using System.Reflection;
using Tgs.GameCommand.Client;
using Tgs.GameInterface.PragmaticPlay.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGameRecordCore(
		this IServiceCollection services,
		Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
		Action<GameInterfacePragmaticPlayClientOptions, IServiceProvider> gameInterfacePragmaticPlayClientOptions)
		=> services
		.RegisterAllServiceUtilityTypes(
			Assembly.GetExecutingAssembly(),
			ServiceLifetime.Transient)
		.AddMemoryCache()
		.AddGameCommandClient(gameCommandClientOptions)
		.AddGameInterfacePragmaticPlayClient(gameInterfacePragmaticPlayClientOptions);
}
