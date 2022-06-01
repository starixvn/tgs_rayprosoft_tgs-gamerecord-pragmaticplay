namespace Tgs.GameRecord.PragmaticPlayTransferService.ViewModels;

public record GameRecordResultViewModel(
    int SuccessCount,
    int FailureCount,
    DateTimeOffset? LastGameRecordTime,
    DateTimeOffset? NextTime);
