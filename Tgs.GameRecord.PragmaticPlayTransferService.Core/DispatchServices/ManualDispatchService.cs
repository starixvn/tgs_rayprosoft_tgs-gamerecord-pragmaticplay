using Microsoft.Extensions.Options;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.JobWorkerServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.DispatchServices;

internal class ManualDispatchService : IGetService<ManualDispatchQuery, ManualDispatchResult>
{
    private readonly GameRecordInfoOptions _gameRecordInfoOptions;
    private readonly IGetService<JobWorkerQuery, JobWorkerResult> _jobWorkerService;
    private readonly IUtensil _utensil;

    public ManualDispatchService(
        IOptions<GameRecordInfoOptions> options,
        IGetService<JobWorkerQuery, JobWorkerResult> jobWorkerService,
        IUtensil utensil)
    {
        _gameRecordInfoOptions = options.Value;
        _jobWorkerService = jobWorkerService ?? throw new ArgumentNullException(nameof(jobWorkerService));
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
    }

    public async ValueTask<ManualDispatchResult> GetAsync(
        ManualDispatchQuery query,
        CancellationToken cancellationToken = default)
    {
        var gameProviderId = await _utensil
            .GetGameProviderIdAsync()
            .ConfigureAwait(false);

        if (!gameProviderId.HasValue)
        {
            return new ManualDispatchResult(
                Success: false,
                Message: $"The game provider cannot be found.({_gameRecordInfoOptions.GameProviderCode})");
        }

        var jobWorkerQuery = new JobWorkerQuery(
            GameProviderId: gameProviderId.Value,
            GameProviderCode: _gameRecordInfoOptions.GameProviderCode,
            GameDataType: query.GameDataType,
            StartTime: query.StartTime,
            EndTime: query.EndTime);

        var result = await _jobWorkerService
                .GetAsync(
                    query: jobWorkerQuery,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

        return new ManualDispatchResult(
                Success: true,
                Message: string.Empty,
                NextTime: result.NextTime,
                LastGameRecordTime: result.LastGameRecordTime);
    }
}
