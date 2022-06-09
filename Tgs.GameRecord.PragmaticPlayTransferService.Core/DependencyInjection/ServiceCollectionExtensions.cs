// ReSharper disable CheckNamespace

using System.Reflection;
using Tgs.GameProviders.Client;
using Tgs.GameProviders.QueryServer.Client;
using Tgs.GameRecord.PragmaticPlayTransferService.Core;
using Tgs.GameRecordSchedules.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameRecordServiceCore(
        this IServiceCollection services,
        Action<GameRecordInfoOptions, IServiceProvider> configure,
        Action<GameProvidersClientOptions, IServiceProvider> gameProvidersClientOptions,
        Action<GameProvidersQueryServerClientOptions, IServiceProvider> gameProvidersQueryServerClientOptions,
        Action<GameRecordSchedulesClientOptions, IServiceProvider> gameRecordSchedulesClientOptions)
        => services
            .AddOptions<GameRecordInfoOptions>()
            .Configure(configure)
            .Services
            .RegisterAllServiceUtilityTypes(
                Assembly.GetExecutingAssembly(),
                ServiceLifetime.Transient)
            .AddGameProvidersClient(gameProvidersClientOptions)
            .AddGameProvidersQueryServerClient(gameProvidersQueryServerClientOptions)
            .AddGameRecordSchedulesClient(gameRecordSchedulesClientOptions)
            .AddScoped<ISystemClock, SystemClock>()
            .AddTransient<IGameRecordScheduleService, GameRecordScheduleService>()
            .AddTransient<IUtensil, UtensilRepository>()
            .Decorate<IUtensil>((inner, sp) =>
                ActivatorUtilities.CreateInstance<UtensilMemoryCachedRepository>(sp,
                    inner, TimeSpan.FromHours(1)));
}
