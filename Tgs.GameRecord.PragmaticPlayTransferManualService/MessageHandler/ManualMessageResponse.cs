namespace Tgs.GameRecord.PragmaticPlayTransferManualService.MessageHandler;

public record ManualMessageResponse(
    bool Success = false,
    string Message = "",
    DateTimeOffset? NextTime = null,
    DateTimeOffset? LastGameRecordTime = null);
