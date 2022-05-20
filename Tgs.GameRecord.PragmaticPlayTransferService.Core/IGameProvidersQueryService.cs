using Tgs.GameProviders.QueryServer.Models;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public interface IGameProvidersQueryService
{
    ValueTask<Guid?> GetBrandGetGameProviderConfigurationIdAsync(Guid brandId, Guid gameProviderId);

    ValueTask<int?> GetGameIdAsync(Guid gameProviderId, string gameExternalId);

    ValueTask<Guid?> GetGameProviderIdAsync(string gameProviderCode);

    ValueTask<PlayerModel?> GetPlayerAsync(string playerCode);
}
