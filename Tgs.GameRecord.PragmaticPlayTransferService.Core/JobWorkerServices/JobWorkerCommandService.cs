using Microsoft.Extensions.Logging;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.JobWorkerServices;

internal class JobWorkerCommandService : IGetService<JobWorkerQuery, JobWorkerResult>
{
    private readonly IGetService<GameRecordQuery, GameRecordResult> _gameRecordQueryService;
    private readonly ILogger<JobWorkerCommandService> _logger;

    public JobWorkerCommandService(
        ILogger<JobWorkerCommandService> logger,
        IGetService<GameRecordQuery, GameRecordResult> gameRecordQueryService)
    {
        _gameRecordQueryService = gameRecordQueryService ?? throw new ArgumentNullException(nameof(gameRecordQueryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask<JobWorkerResult> GetAsync(
        JobWorkerQuery query,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset? lastGameRecordTime = null;
        var nextTime = query.StartTime;
        try
        {
            while (nextTime < query.EndTime)
            {
                var result = await _gameRecordQueryService
                    .GetAsync(new GameRecordQuery(query.GameDataType, nextTime), cancellationToken)
                    .ConfigureAwait(false);

                if (result.Success && result.NextTime != null)
                {
                    nextTime = result.NextTime.Value;
                    lastGameRecordTime = result.LastGameRecordTime;
                }
                else
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "發生例外");
        }

        return new JobWorkerResult(
            NextTime: nextTime,
            LastGameRecordTime: lastGameRecordTime);
    }
}
