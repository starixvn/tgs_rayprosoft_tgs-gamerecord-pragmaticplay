using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.PragmaticPlayTransferService.Core;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;
using Tgs.GameRecord.PragmaticPlayTransferService.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Controllers;

[Route("PPTF")]
[ApiController]
public class PragmaticPlayTransferController : ControllerBase
{
    /// <summary>
    /// Games the record asynchronous.
    /// </summary>
    /// <param name="getService">The service.</param>
    /// <param name="viewModel">The query.</param>
    /// <returns></returns>
    /// <remarks>
    /// <b>Example</b><br/>
    ///
    ///     Post
    ///         {
    ///             "dataType": "RNG",
    ///             "startTime": "2021-11-30T16:00:00+00:00"
    ///         }
    /// </remarks>
    [HttpPost]
    public async ValueTask<GameRecordResultViewModel> GameRecordAsync(
        [FromServices] IGetService<GameRecordQuery, GameRecordResult> getService,
        GameRecordQueryViewModel viewModel)
    {
        var result = await getService
            .GetAsync(
                new GameRecordQuery(
                    viewModel.DataType,
                    viewModel.StartTime))
            .ConfigureAwait(false);

        return new GameRecordResultViewModel(
            result.SuccessCount,
            result.FailureCount,
            result.LastGameRecordTime,
            result.NextTime);
    }
}
