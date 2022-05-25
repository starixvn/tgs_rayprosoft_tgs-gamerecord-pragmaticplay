namespace Tgs.GameRecord.PragmaticPlayTransferService.MessageHandler;

public record ManualMessageResponse(
    bool Success,
    int Count,
    int SuccessCount,
    int FailureCount,
    DateTimeOffset? LastGameRecordTime,
    DateTimeOffset? NextTime);
