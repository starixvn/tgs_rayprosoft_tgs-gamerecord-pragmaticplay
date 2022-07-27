using Tgs.GameProviders.Client;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.JobWorkerServices;

public record JobWorkerQuery(
    Guid GameProviderId,
    string GameProviderCode,
    PragmaticPlayTransferGameDataType GameDataType,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
