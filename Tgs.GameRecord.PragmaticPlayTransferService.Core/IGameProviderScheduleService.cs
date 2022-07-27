using Tgs.GameProviders.Client;
using Tgs.GameRecordSchedules.Protos;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public interface IGameRecordScheduleService
{
    ValueTask<GameRecordScheduleReply?> GetAsync(
       PragmaticPlayTransferGameDataType gameDataType,
       CancellationToken cancellationToken = default);

    ScheduleConfig GetConfig(GameRecordScheduleReply gameRecordSchedule);

    ValueTask<GameRecordScheduleReply?> InitAsync(
      Guid gameProviderId,
      PragmaticPlayTransferGameDataType gameDataType,
      CancellationToken cancellationToken = default);

    ValueTask<GameRecordScheduleReply?> UpdateAsync(
      GameRecordScheduleReply gameRecordSchedule,
      PragmaticPlayTransferGameDataType gameDataType,
      DateTimeOffset nextTime,
      DateTimeOffset? lastGameRecordTime = null,
      CancellationToken cancellationToken = default);
}
