namespace Tgs.GameRecord.PragmaticPlayTransferService.MessageHandler;

public record ScheduleMessageModel(
        Guid GameProviderId,
        string GameProviderCode,
        string GameDataType);
