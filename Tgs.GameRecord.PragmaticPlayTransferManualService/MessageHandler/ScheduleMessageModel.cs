namespace Tgs.GameRecord.PragmaticPlayTransferManualService.MessageHandler;

public record ScheduleMessageModel(
        Guid GameProviderId,
        string GameProviderCode,
        string GameDataType);
