namespace Tgs.GameRecord.PragmaticPlay.Core.ApplicationServices;

public record SchedulePullRecordCommand(
    Uri ApiUrl,
    string SecureLogin,
    string SecretKey,
    string DataType);
