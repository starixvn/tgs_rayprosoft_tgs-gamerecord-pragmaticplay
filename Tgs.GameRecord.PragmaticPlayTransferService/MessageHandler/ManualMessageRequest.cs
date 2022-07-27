using Tgs.GameProviders.Client;

namespace Tgs.GameRecord.PragmaticPlayTransferService.MessageHandler;

public record ManualMessageRequest(
    PragmaticPlayTransferGameDataType GameDataType,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
