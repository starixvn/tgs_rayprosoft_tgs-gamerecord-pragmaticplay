using Tgs.GameCommand.Client;
using Tgs.GameCommand.Common;
using Tgs.GameRecord.PragmaticPlay.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceRegisterExtensions
{
    public static async Task<WebApplication> UseHangfireRegisterAsync(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var sp = scope.ServiceProvider;
            var hangfireSchedule = scope.ServiceProvider.GetRequiredService<IHangfireUtensil>();
            await hangfireSchedule.JobInitAsync().ConfigureAwait(false);
        }

        return app;
    }

    public static async Task<WebApplication> UseRegisterAsync(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var gameRecordInfo = app.Configuration.GetSection("GameRecordInfo").Get<GameRecordInfoOptions>()!;
            var utensil = scope.ServiceProvider.GetRequiredService<IUtensil>();
            var gameProviderId = await utensil.GetGameProviderIdAsync().ConfigureAwait(false);
            var client = scope.ServiceProvider.GetRequiredService<IGameCommandClient>();
            var games = await client
                .GameQueryAsync(
                    gameProviderCode: gameRecordInfo.GameProviderCode)
                .ToArrayAsync()
                .ConfigureAwait(false)
                ?? Enumerable.Empty<GameInfo>();
            foreach (var game in games)
            {
                _ = await utensil
                    .GetGameIdAsync(
                        gameProviderId: gameProviderId,
                        gameExternalId: game.GameExternalId)
                    .ConfigureAwait(false);
            }
        }

        return app;
    }
}
