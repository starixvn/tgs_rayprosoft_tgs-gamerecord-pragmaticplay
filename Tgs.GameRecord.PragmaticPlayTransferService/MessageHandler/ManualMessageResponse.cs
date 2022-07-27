namespace Tgs.GameRecord.PragmaticPlayTransferService.MessageHandler;

public record ManualMessageResponse(
    bool Success = false,
    string Message = "",
    DateTimeOffset? NextTime = null,
    DateTimeOffset? LastGameRecordTime = null);
