using Tgs.GameProviders.QueryServer.Models;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public interface IUtensil
{
    ValueTask<Guid?> GetBrandGetGameProviderConfigurationIdAsync(
        Guid brandId,
        Guid gameProviderId,
        CancellationToken cancellationToken = default);

    ValueTask<int?> GetGameIdAsync(
        Guid gameProviderId,
        string gameExternalId,
        CancellationToken cancellationToken = default);

    ValueTask<Guid> GetGameProviderIdAsync(
        CancellationToken cancellationToken = default);

    ValueTask<PlayerModel?> GetPlayerAsync(
        string playerCode,
        CancellationToken cancellationToken = default);
}
