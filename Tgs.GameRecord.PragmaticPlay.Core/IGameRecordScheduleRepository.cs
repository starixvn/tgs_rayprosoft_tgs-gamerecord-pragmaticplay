using Tgs.GameRecord.PragmaticPlay.Core.Models;

namespace Tgs.GameRecord.PragmaticPlay.Core;

public interface IGameRecordScheduleRepository
{
	ValueTask<GameRecordScheduleInfo?> GetAsync(string secureLogin, string dataType, CancellationToken cancellationToken = default);

	ValueTask<GameRecordScheduleInfo> AddAsync(string secureLogin, string dataType, DateTimeOffset nextTime, CancellationToken cancellationToken = default);

	ValueTask InitializationAsync(CancellationToken cancellationToken = default);

	ValueTask UpdateAsync(string secureLogin, string dataType, DateTimeOffset nextTime, CancellationToken cancellationToken = default);
}
