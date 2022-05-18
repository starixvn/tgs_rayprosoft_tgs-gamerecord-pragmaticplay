using Tgs.GameProviders.Client;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public record ScheduleConfig
{
    public decimal IntervalMinute { get; set; }
    public decimal IntervalMinuteMax { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public DateTimeOffset LastGameRecordTime { get; set; }
    public PragmaticPlayTransferGameDataType DataType { get; set; } = default!;
    public DateTimeOffset NextTime { get; set; }
}
