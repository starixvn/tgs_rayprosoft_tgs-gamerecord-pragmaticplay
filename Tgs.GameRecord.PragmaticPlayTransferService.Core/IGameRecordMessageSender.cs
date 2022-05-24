namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public interface IGameRecordMessageSender
{
    ValueTask SendGameRoundAsync(GameRoundMessageRequest messageRequest);
}
