namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;
    public string GameProviderConfiguration { get; set; } = default!;
    public int IntervalMinute { get; set; }
    public int IntervalMinuteMax { get; set; }

    public int LockSeconds { get; set; }
}
