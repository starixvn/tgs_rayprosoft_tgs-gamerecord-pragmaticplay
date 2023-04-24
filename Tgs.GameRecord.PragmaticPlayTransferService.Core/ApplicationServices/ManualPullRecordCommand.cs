namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;

public record ManualPullRecordCommand(
    Uri ApiUrl,
    string SecureLogin,
    string SecretKey,
    string DataType,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
