using Tgs.GameRecord.PragmaticPlayTransferManualService;

namespace Tgs.GameRecord.PragmaticPlayTransferManualService.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameRecordAwcService(
        this IServiceCollection services,
        Action<RedisOptions, IServiceProvider> redisDBconfigure)
        => services
            .AddOptions<RedisOptions>()
            .Configure(redisDBconfigure)
            .Services
            .AddSingleton<RedisConnectionManager>();
}
