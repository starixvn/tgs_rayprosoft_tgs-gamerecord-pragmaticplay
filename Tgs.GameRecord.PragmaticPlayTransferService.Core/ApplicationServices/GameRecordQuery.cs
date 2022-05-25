using Tgs.GameProviders.Client;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;

public record GameRecordQuery(
    PragmaticPlayTransferGameDataType DataType,
    DateTimeOffset StartTime);
