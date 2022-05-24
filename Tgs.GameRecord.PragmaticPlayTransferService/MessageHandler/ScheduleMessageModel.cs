namespace Tgs.GameRecord.PragmaticPlayTransferService.MessageHandler;

public record ScheduleMessageModel(
    string GameProviderCode,
    DateTimeOffset SendDateTime);
