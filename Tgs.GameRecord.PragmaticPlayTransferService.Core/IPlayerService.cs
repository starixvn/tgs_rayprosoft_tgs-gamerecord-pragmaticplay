namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public interface IPlayerService
{
    ValueTask<PlayerInfo?> GetAsync(Guid gameProviderId, string playerCode);
}
