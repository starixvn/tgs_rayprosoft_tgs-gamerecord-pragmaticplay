namespace Tgs.GameRecord.PragmaticPlayTransferManualService.ViewModels;

public record ManualDispatchResultViewModel(
    bool Success = false,
    string Message = "",
    DateTimeOffset? NextTime = null,
    DateTimeOffset? LastGameRecordTime = null);
