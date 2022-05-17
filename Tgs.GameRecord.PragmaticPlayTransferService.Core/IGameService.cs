namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public interface IGameService
{
    ValueTask<int?> GetGameIdAsync(Guid gameProviderId, string gameExternalId);
}
