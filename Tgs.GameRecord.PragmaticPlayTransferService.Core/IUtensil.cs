using Tgs.GameCommand.Common;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public interface IUtensil
{
    ValueTask<Guid?> GetBrandGameProviderConfigurationIdAsync(
        Guid brandId,
        Guid gameProviderId,
        CancellationToken cancellationToken = default);

    ValueTask<int?> GetGameIdAsync(
        Guid gameProviderId,
        string gameExternalId,
        CancellationToken cancellationToken = default);

    string GetGameProviderCode();

    ValueTask<Guid> GetGameProviderIdAsync(
            CancellationToken cancellationToken = default);

    ValueTask<PlayerInfo?> GetPlayerAsync(
        string playerCode,
        CancellationToken cancellationToken = default);

    bool IsRun(DateTimeOffset nextTime, DateTimeOffset endTime);
}
