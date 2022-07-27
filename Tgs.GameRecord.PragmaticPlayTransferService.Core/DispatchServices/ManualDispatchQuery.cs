using Tgs.GameProviders.Client;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.DispatchServices;

public record ManualDispatchQuery(
    PragmaticPlayTransferGameDataType GameDataType,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
