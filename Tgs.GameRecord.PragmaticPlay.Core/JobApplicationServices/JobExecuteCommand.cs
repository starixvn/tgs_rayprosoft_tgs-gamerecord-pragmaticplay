namespace Tgs.GameRecord.PragmaticPlay.Core.JobApplicationServices;

public record JobExecuteCommand(
    string GameProviderCode,
    string Name,
    Uri ApiUrl,
    string SecureLogin,
    string SecretKey,
    string DataType);
