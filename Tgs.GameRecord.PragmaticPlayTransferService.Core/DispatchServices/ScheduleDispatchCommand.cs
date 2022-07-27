namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.DispatchServices;

public record ScheduleDispatchCommand(
    Guid GameProviderId,
    string GameProviderCode,
    string GameDataType);
