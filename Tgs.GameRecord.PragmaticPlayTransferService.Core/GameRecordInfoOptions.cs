namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;
    public string GameProviderConfiguration { get; set; } = default!;
    public double IntervalMinutes { get; set; } = 5d;
    public double IntervalMinutesMax { get; set; } = 10d;
    public double IntervalMinutesMinimum { get; set; } = 1d;
    public double VirtualSportsIntervalMinutes { get; set; } = 10d;
    public double LiveDealerIntervalMinutes { get; set; } = 1d;
    public double LockMinute { get; set; } = 8d;
}
