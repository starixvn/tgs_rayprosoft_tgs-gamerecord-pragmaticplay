namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.DispatchServices;

public record ManualDispatchResult(
    bool Success = false,
    string Message = "",
    DateTimeOffset? NextTime = null,
    DateTimeOffset? LastGameRecordTime = null);
