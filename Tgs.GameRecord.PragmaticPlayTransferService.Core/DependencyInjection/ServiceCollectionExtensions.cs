// ReSharper disable CheckNamespace

using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using StackExchange.Redis;
using Tgs.GameCommand.Client;
using Tgs.GameProviders.Client;
using Tgs.GameRecord.PragmaticPlayTransferService.Core;
using Tgs.GameRecordSchedules.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameRecordServiceCore(
        this IServiceCollection services,
        Action<GameRecordInfoOptions, IServiceProvider> configure,
        Action<GameProvidersClientOptions, IServiceProvider> gameProvidersClientOptions,
        Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
        Action<GameRecordSchedulesClientOptions, IServiceProvider> gameRecordSchedulesClientOptions,
        Func<IServiceProvider, IDatabase> redisDatabaseResolver)
        => services
            .AddOptions<GameRecordInfoOptions>()
            .Configure(configure)
            .Services
            .RegisterAllServiceUtilityTypes(
                Assembly.GetExecutingAssembly(),
                ServiceLifetime.Transient)
            .AddGameProvidersClient(gameProvidersClientOptions)
            .AddGameCommandClient(gameCommandClientOptions)
            .AddGameRecordSchedulesClient(gameRecordSchedulesClientOptions)
            .AddSingleton<ISystemClock, SystemClock>()
            .AddTransient<IGameRecordScheduleService, GameRecordScheduleService>()
            .AddTransient<IScheduleDispatch>(sp => ActivatorUtilities.CreateInstance<ScheduleDispatch>(sp, redisDatabaseResolver(sp)))
            .AddTransient<IUtensil, UtensilRepository>()
            .Decorate<IUtensil>((inner, sp) =>
                ActivatorUtilities.CreateInstance<UtensilMemoryCachedRepository>(sp, inner));
}
