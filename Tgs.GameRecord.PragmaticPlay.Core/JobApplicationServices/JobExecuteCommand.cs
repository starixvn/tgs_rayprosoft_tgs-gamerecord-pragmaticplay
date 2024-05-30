namespace Tgs.GameRecord.PragmaticPlay.Core.JobApplicationServices;

public record JobExecuteCommand(
	Uri ApiUrl,
	string SecureLogin,
	string SecretKey,
	string DataType);
