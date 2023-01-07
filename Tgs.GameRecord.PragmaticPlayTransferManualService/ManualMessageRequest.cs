using Tgs.GameProviders.Client;

namespace Tgs.GameRecord.PragmaticPlayTransferManualService;

public record ManualMessageRequest(
    PragmaticPlayTransferGameDataType GameDataType,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
