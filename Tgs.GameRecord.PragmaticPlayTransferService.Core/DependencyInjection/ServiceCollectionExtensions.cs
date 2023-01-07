// ReSharper disable CheckNamespace

using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Tgs.GameCommand.Client;
using Tgs.GameProviders.Client;
using Tgs.GameRecord.PragmaticPlayTransferService.Core;
using Tgs.GameRecordSchedules.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameRecordCore(
        this IServiceCollection services,
        Action<RedisOptions, IServiceProvider> redisDBconfigure,
        Action<GameRecordInfoOptions, IServiceProvider> gameRecordInfoOptions,
        Action<MongoSettingOptions, IServiceProvider> mongoSettingOptions,
        Action<GameProvidersClientOptions, IServiceProvider> gameProvidersClientOptions,
        Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
        Action<GameRecordSchedulesClientOptions, IServiceProvider> gameRecordSchedulesClientOptions)
        => services
            .AddOptions<RedisOptions>()
            .Configure(redisDBconfigure)
            .Services
            .AddOptions<GameRecordInfoOptions>()
            .Configure(gameRecordInfoOptions)
            .Services
            .AddOptions<MongoSettingOptions>()
            .Configure(mongoSettingOptions)
            .Services
            .RegisterAllServiceUtilityTypes(
                Assembly.GetExecutingAssembly(),
                ServiceLifetime.Transient)
            .AddGameProvidersClient(gameProvidersClientOptions)
            .AddGameCommandClient(gameCommandClientOptions)
            .AddGameRecordSchedulesClient(gameRecordSchedulesClientOptions)
            .AddMemoryCache()
            .AddSingleton<RedisConnectionManager>()
            .AddSingleton<ISystemClock, SystemClock>()
            .AddTransient<IGameRecordScheduleService, GameRecordScheduleService>()
            .AddTransient<IScheduleDispatch>(sp => ActivatorUtilities.CreateInstance<ScheduleDispatch>(sp, sp.GetRequiredService<RedisConnectionManager>().Database))
            .AddTransient<IUtensil>(sp => ActivatorUtilities.CreateInstance<UtensilRepository>(sp, TimeSpan.FromHours(1)));
}
