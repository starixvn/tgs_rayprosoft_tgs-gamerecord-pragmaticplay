namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public interface IGameRecordMessageSender
{
    ValueTask SendGameRoundAsync(GameRoundData data);
}
