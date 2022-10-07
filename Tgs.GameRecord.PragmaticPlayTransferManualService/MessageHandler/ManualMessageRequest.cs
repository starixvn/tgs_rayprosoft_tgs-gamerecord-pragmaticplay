using Tgs.GameProviders.Client;

namespace Tgs.GameRecord.PragmaticPlayTransferManualService.MessageHandler;

public record ManualMessageRequest(
    PragmaticPlayTransferGameDataType GameDataType,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
