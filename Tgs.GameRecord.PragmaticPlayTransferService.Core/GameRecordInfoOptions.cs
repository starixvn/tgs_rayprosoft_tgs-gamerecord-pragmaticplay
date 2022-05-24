namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;
    public string GameProviderConfiguration { get; set; } = default!;
    public decimal IntervalMinute { get; set; }
    public decimal IntervalMinuteMax { get; set; }
}
