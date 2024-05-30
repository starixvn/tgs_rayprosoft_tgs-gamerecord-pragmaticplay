namespace Tgs.GameRecord.PragmaticPlay.Core;

public interface IUtensilRepository
{
	bool IsRun(DateTimeOffset nextTime, DateTimeOffset endTime, DateTimeOffset currentTime);
}
