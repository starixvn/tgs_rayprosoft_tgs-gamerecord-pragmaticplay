using Microsoft.AspNetCore.Builder;
using Tgs.GameRecord.PragmaticPlayTransferService.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class GameRecordRegisterExtensions
{
    public static async Task<WebApplication> UseGameRecordRegister(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var utensil = scope.ServiceProvider.GetRequiredService<IUtensil>();
            var gameProviderId = await utensil.GetGameProviderIdAsync().ConfigureAwait(false);
        }

        return app;
    }
}
