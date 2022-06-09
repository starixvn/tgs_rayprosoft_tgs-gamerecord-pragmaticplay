using Tgs.GameProviders.Client;
using Tgs.GameRecordSchedules.Protos;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public interface IGameRecordScheduleService
{
    ValueTask<GameRecordScheduleReply?> GetAsync(
        PragmaticPlayTransferGameDataType gameDataType,
        CancellationToken cancellationToken = default);

    ScheduleConfig GetConfig(
        GameRecordScheduleReply gameProviderSchedule);

    ValueTask<GameRecordScheduleReply?> InitAsync(
        Guid gameProviderId,
        PragmaticPlayTransferGameDataType gameDataType,
        CancellationToken cancellationToken = default);

    ValueTask<GameRecordScheduleReply?> UpdateAsync(
        GameRecordScheduleReply gameProviderSchedule,
        DateTimeOffset? lastGameRecordTime,
        DateTimeOffset nextTime,
        DateTimeOffset sysTime,
        CancellationToken cancellationToken = default);
}
