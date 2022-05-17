namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public interface IGameProviderService
{
    ValueTask<Guid> GetGameProviderIdAsync(string gameProviderCode);
}
