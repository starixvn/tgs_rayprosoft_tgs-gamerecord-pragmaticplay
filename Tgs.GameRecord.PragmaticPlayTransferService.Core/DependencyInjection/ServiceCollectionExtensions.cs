// ReSharper disable CheckNamespace

using System.Reflection;
using Tgs.GameCommand.Client;
using Tgs.GameInterface.PragmaticPlay.Client;
using Tgs.GameRecord.PragmaticPlayTransferService.Core;
using Tgs.GameRecordSchedules.Client;
using Tgs.GameRounds.Schedule.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameRecordCore(
        this IServiceCollection services,
        Action<RedisOptions, IServiceProvider> redisDBconfigure,
        Action<GameRecordInfoOptions, IServiceProvider> gameRecordInfoOptions,
        Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
        Action<GameRoundsScheduleClientOptions, IServiceProvider> gameRoundsScheduleClientOptions,
        Action<GameInterfacePragmaticPlayClientOptions, IServiceProvider> gameInterfacePragmaticPlayClientOptions,
        Action<GameRecordSchedulesClientOptions, IServiceProvider> gameRecordSchedulesClientOptions)
        => services
        .AddOptions<RedisOptions>()
        .Configure(redisDBconfigure)
        .Services
        .AddOptions<GameRecordInfoOptions>()
        .Configure(gameRecordInfoOptions)
        .Services
        .RegisterAllServiceUtilityTypes(
            Assembly.GetExecutingAssembly(),
            ServiceLifetime.Transient)
        .AddMemoryCache()
        .AddSingleton<RedisConnectionManager>()
        .AddGameCommandClient(gameCommandClientOptions)
        .AddGameRoundsScheduleClient(gameRoundsScheduleClientOptions)
        .AddGameInterfacePragmaticPlayClient(gameInterfacePragmaticPlayClientOptions)
        .AddGameRecordSchedulesClient(gameRecordSchedulesClientOptions)
        .AddGameRoundsWorkClient()
        .AddGameRoundsWorkManualClient()
        .AddSingleton<IScheduleFormDispatcher, ScheduleFormDispatcher>()
        .AddSingleton<IScheduleFormLock>(sp => ActivatorUtilities.CreateInstance<ScheduleFormLock>(sp, sp.GetRequiredService<RedisConnectionManager>().Database))
        .AddSingleton<ISchedulePullRecordLock>(sp => ActivatorUtilities.CreateInstance<SchedulePullRecordLock>(sp, sp.GetRequiredService<RedisConnectionManager>().Database))
        .AddTransient<IUtensil>(sp => ActivatorUtilities.CreateInstance<UtensilRepository>(sp, TimeSpan.FromHours(1)))
        .AddSingleton<ISystemClock, SystemClock>()
        .AddSingleton<IGameRecordUtensil, GameRecordUtensil>()
        .AddTransient<IGameRecordScheduleService, GameRecordScheduleService>();
}
