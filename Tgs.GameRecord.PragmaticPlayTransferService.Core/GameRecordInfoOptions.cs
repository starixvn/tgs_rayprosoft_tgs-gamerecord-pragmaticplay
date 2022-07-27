namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;
    public string GameProviderConfiguration { get; set; } = default!;
    public double MaxIntervalMinute { get; set; } = 10d;
    public double IntervalMinute { get; set; } = 10d;
    public double LockMinute { get; set; } = 8.5d;
}
