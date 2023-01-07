namespace Tgs.GameRecord.PragmaticPlayTransferManualService;

public record ManualMessageResponse(
    bool Success = false,
    string Message = "",
    DateTimeOffset? NextTime = null,
    DateTimeOffset? LastGameRecordTime = null);
