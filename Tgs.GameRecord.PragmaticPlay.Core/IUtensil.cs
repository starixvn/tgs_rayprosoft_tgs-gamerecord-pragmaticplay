namespace Tgs.GameRecord.PragmaticPlay.Core;

public interface IUtensil
{
    ValueTask<Guid> GetGameProviderIdAsync(
        CancellationToken cancellationToken = default);

    bool IsRun(DateTimeOffset nextTime, DateTimeOffset endTime, DateTimeOffset currentTime);
}
