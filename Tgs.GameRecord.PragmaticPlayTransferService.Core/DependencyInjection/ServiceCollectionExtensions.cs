// ReSharper disable CheckNamespace

using System.Reflection;
using Tgs.GameProviders.Client;
using Tgs.GameProviders.QueryServer.Client;
using Tgs.GameRecord.PragmaticPlayTransferService.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPragmaticPlayTransferGameRecordService(
        this IServiceCollection services,
        Action<GameProvidersClientOptions, IServiceProvider> gameProvidersClientOptions,
        Action<GameProvidersQueryServerClientOptions, IServiceProvider> gameProvidersQueryServerClientOptions)
        => services.RegisterAllServiceUtilityTypes(
                Assembly.GetExecutingAssembly(),
                ServiceLifetime.Transient)
            .AddGameProvidersClient(gameProvidersClientOptions)
            .AddGameProvidersQueryServerClient(gameProvidersQueryServerClientOptions)
            .AddTransient<IPlayerService, PlayerService>()
            .AddTransient<IGameProviderService, GameProviderService>()
            .AddTransient<IGameService, GameService>();
}
