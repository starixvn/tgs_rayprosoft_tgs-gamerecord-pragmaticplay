using Tgs.GameProviders.Client;

namespace Tgs.GameRecord.PragmaticPlayTransferService.MessageHandler;

public record ManualMessageRequest(
    PragmaticPlayTransferGameDataType DataType,
    DateTimeOffset StartTime);
