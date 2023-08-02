using Tgs.GameRecordSchedules.Protos;

namespace Tgs.GameRecord.PragmaticPlay.Core;

public interface IGameRecordScheduleService
{
    ValueTask<GameRecordScheduleReply?> GetAsync(
       string dataType,
       CancellationToken cancellationToken = default);

    ScheduleConfig GetConfig(GameRecordScheduleReply gameRecordSchedule);

    ValueTask<GameRecordScheduleReply?> InitAsync(
      Guid gameProviderId,
      string dataType,
      CancellationToken cancellationToken = default);

    ValueTask<GameRecordScheduleReply?> UpdateAsync(
      GameRecordScheduleReply gameRecordSchedule,
      string dataType,
      DateTimeOffset nextTime,
      CancellationToken cancellationToken = default);
}
