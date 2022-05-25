namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public record GameRecordResult(
    bool Success,
    int Count,
    int SuccessCount,
    int FailureCount,
    DateTimeOffset? LastGameRecordTime,
    DateTimeOffset? NextTime);
