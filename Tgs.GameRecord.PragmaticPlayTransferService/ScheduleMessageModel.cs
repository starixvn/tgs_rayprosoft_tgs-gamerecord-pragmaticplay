namespace Tgs.GameRecord.PragmaticPlayTransferService;

public record ScheduleMessageModel(
        Guid GameProviderId,
        string GameProviderCode,
        string GameDataType);
