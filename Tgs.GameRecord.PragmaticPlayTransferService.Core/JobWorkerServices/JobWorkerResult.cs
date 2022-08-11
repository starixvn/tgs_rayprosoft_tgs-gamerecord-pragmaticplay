namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.JobWorkerServices;

public record JobWorkerResult(
    DateTimeOffset NextTime,
    DateTimeOffset? LastGameRecordTime = null);
