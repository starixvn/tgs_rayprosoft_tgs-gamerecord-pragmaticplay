using Microsoft.AspNetCore.Mvc;
using Tgs.GameProviders.Client;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.DispatchServices;
using Tgs.GameRecord.PragmaticPlayTransferService.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Controllers;

[Route("PPTF")]
[ApiController]
public class PragmaticPlayTransferController : ControllerBase
{
    /// <summary>
    /// Manuals the dispatch by fishing asynchronous.
    /// </summary>
    /// <param name="service">The service.</param>
    /// <param name="viewModel">The view model.</param>
    /// <returns></returns>
    [HttpPost("Fishing")]
    public async ValueTask<ManualDispatchResultViewModel> ManualDispatchByFishingAsync(
        [FromServices] IGetService<ManualDispatchQuery, ManualDispatchResult> service,
        ManualDispatchQueryViewModel viewModel)
    {
        var query = new ManualDispatchQuery(
            GameDataType: PragmaticPlayTransferGameDataType.Fishing,
            StartTime: viewModel.StartTime,
            EndTime: viewModel.EndTime);

        var result = await service
            .GetAsync(query)
            .ConfigureAwait(false);

        return new ManualDispatchResultViewModel(
            result.Success,
            result.Message,
            result.NextTime,
            result.LastGameRecordTime);
    }

    /// <summary>
    /// Manuals the dispatch by live dealer asynchronous.
    /// </summary>
    /// <param name="service">The service.</param>
    /// <param name="viewModel">The view model.</param>
    /// <returns></returns>
    [HttpPost("LiveDealer")]
    public async ValueTask<ManualDispatchResultViewModel> ManualDispatchByLiveDealerAsync(
        [FromServices] IGetService<ManualDispatchQuery, ManualDispatchResult> service,
        ManualDispatchQueryViewModel viewModel)
    {
        var query = new ManualDispatchQuery(
            GameDataType: PragmaticPlayTransferGameDataType.LiveDealer,
            StartTime: viewModel.StartTime,
            EndTime: viewModel.EndTime);

        var result = await service
            .GetAsync(query)
            .ConfigureAwait(false);

        return new ManualDispatchResultViewModel(
            result.Success,
            result.Message,
            result.NextTime,
            result.LastGameRecordTime);
    }

    /// <summary>
    /// Manuals the dispatch by RNG asynchronous.
    /// </summary>
    /// <param name="service">The service.</param>
    /// <param name="viewModel">The view model.</param>
    /// <returns></returns>
    [HttpPost("RNG")]
    public async ValueTask<ManualDispatchResultViewModel> ManualDispatchByRNGAsync(
        [FromServices] IGetService<ManualDispatchQuery, ManualDispatchResult> service,
        ManualDispatchQueryViewModel viewModel)
    {
        var query = new ManualDispatchQuery(
            GameDataType: PragmaticPlayTransferGameDataType.RNG,
            StartTime: viewModel.StartTime,
            EndTime: viewModel.EndTime);

        var result = await service
            .GetAsync(query)
            .ConfigureAwait(false);

        return new ManualDispatchResultViewModel(
            result.Success,
            result.Message,
            result.NextTime,
            result.LastGameRecordTime);
    }

    /// <summary>
    /// Manuals the dispatch by virtual sports asynchronous.
    /// </summary>
    /// <param name="service">The service.</param>
    /// <param name="viewModel">The view model.</param>
    /// <returns></returns>
    [HttpPost("VirtualSports")]
    public async ValueTask<ManualDispatchResultViewModel> ManualDispatchByVirtualSportsAsync(
        [FromServices] IGetService<ManualDispatchQuery, ManualDispatchResult> service,
        ManualDispatchQueryViewModel viewModel)
    {
        var query = new ManualDispatchQuery(
            GameDataType: PragmaticPlayTransferGameDataType.VirtualSports,
            StartTime: viewModel.StartTime,
            EndTime: viewModel.EndTime);

        var result = await service
            .GetAsync(query)
            .ConfigureAwait(false);

        return new ManualDispatchResultViewModel(
            result.Success,
            result.Message,
            result.NextTime,
            result.LastGameRecordTime);
    }
}
