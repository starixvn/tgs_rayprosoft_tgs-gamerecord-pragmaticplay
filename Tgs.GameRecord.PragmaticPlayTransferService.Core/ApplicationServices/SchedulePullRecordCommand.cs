namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;

public record SchedulePullRecordCommand(
    Uri ApiUrl,
    string SecureLogin,
    string SecretKey,
    string DataType);
