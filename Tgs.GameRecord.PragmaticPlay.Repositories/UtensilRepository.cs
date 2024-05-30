using Tgs.GameRecord.PragmaticPlay.Core;

namespace Tgs.GameRecord.PragmaticPlay.Repositories;

internal class UtensilRepository() : IUtensilRepository
{
	public bool IsRun(DateTimeOffset nextTime, DateTimeOffset endTime, DateTimeOffset currentTime)
		=> nextTime < endTime && nextTime < currentTime && endTime.Subtract(nextTime).TotalSeconds > 30;
}
