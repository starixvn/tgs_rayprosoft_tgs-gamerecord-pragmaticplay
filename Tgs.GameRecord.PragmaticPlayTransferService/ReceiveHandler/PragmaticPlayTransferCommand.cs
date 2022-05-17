namespace Tgs.GameRecord.PragmaticPlayTransferService.ReceiveHandler;

public record PragmaticPlayTransferCommand(
    string GameProviderCode,
    DateTimeOffset SendDateTime);
