using Tgs.GameProviders.Client;

namespace Tgs.GameRecord.PragmaticPlayTransferService.ViewModels;

public record GameRecordQueryViewModel(
    PragmaticPlayTransferGameDataType DataType,
    DateTimeOffset StartTime);
