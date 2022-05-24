namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public interface ISystemClock
{
    DateTimeOffset CurrentTime { get; }
}
