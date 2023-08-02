namespace Tgs.GameRecord.PragmaticPlay.Core;

public interface ISystemClock
{
    DateTimeOffset CurrentTime { get; }
}
