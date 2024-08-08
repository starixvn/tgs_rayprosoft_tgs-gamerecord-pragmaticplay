using Tgs.GameRecord.PragmaticPlay.Core.ApplicationServices;

namespace Tgs.GameRecord.PragmaticPlay.Core;

public interface IManualPullRecordDispatcher
{
	ValueTask PushCommandAsync(ManualPullRecordCommand command, CancellationToken cancellationToken = default);
}
