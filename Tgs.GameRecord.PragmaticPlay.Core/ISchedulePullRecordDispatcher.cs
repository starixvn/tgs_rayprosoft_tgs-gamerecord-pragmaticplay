using Tgs.GameRecord.PragmaticPlay.Core.ApplicationServices;

namespace Tgs.GameRecord.PragmaticPlay.Core;

public interface ISchedulePullRecordDispatcher
{
	ValueTask PushCommandAsync(SchedulePullRecordCommand command, CancellationToken cancellationToken = default);
}
