using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.PragmaticPlay.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlay.Controllers;

[Route("[controller]")]
[ApiController]
public class PragmaticPlayTransferController : ControllerBase
{
    /// <summary>
    /// Games the record asynchronous.
    /// </summary>
    /// <param name="service">The service.</param>
    /// <param name="source">The source.</param>
    /// <returns></returns>
    /// <remarks>
    /// <b>Example</b><br />
    ///
    ///     Post
    ///
    ///         LiveDealer Example
    ///         {
    ///             "apiUrl": "https://api.prerelease-env.biz/",
    ///             "secureLogin": "tgs_tgstfw",
    ///             "secretKey": "testKey",
    ///             "dataType": "LC",
    ///             "startTime": "2023-04-29T15:00:00+08:00",
    ///             "endTime": "2023-04-29T16:00:08+00:00"
    ///         }
    ///
    ///         RNG Example
    ///         {
    ///             "apiUrl": "https://api.prerelease-env.biz/",
    ///             "secureLogin": "tgs_tgstfw",
    ///             "secretKey": "testKey",
    ///             "dataType": "RNG",
    ///             "startTime": "2023-04-29T15:00:00+08:00",
    ///             "endTime": "2023-04-29T16:00:08+00:00"
    ///         }
    ///
    ///         VSB Example
    ///         {
    ///             "apiUrl": "https://api.prerelease-env.biz/",
    ///             "secureLogin": "tgs_tgstfw",
    ///             "secretKey": "testKey",
    ///             "dataType": "VSB",
    ///             "startTime": "2023-04-29T15:00:00+08:00",
    ///             "endTime": "2023-04-29T16:00:08+00:00"
    ///         }
    /// </remarks>
    [HttpPost("manual")]
    public ValueTask GameRecordAsync(
       [FromServices] ICommandService<ManualPullRecordCommand> service,
       [FromBody] ManualPullRecordCommand source)
        => service
        .ExecuteAsync(
            command: source,
            cancellationToken: HttpContext.RequestAborted);

    /// <summary>
    /// Games the record asynchronous.
    /// </summary>
    /// <param name="service">The service.</param>
    /// <param name="source">The source.</param>
    /// <returns></returns>
    /// <remarks>
    /// <b>Example</b><br />
    ///
    ///     Post
    ///
    ///         LiveDealer Example
    ///         {
    ///             "apiUrl": "https://api.prerelease-env.biz/",
    ///             "secureLogin": "tgs_tgstfw",
    ///             "secretKey": "testKey",
    ///             "dataType": "LC"
    ///         }
    ///
    ///         RNG Example
    ///         {
    ///             "apiUrl": "https://api.prerelease-env.biz/",
    ///             "secureLogin": "tgs_tgstfw",
    ///             "secretKey": "testKey",
    ///             "dataType": "RNG"
    ///         }
    ///
    ///         VSB Example
    ///         {
    ///             "apiUrl": "https://api.prerelease-env.biz/",
    ///             "secureLogin": "tgs_tgstfw",
    ///             "secretKey": "testKey",
    ///             "dataType": "VSB"
    ///         }
    /// </remarks>
    [HttpPost("schedule")]
    public ValueTask GameRecordAsync(
       [FromServices] ICommandService<SchedulePullRecordCommand> service,
       [FromBody] SchedulePullRecordCommand source)
        => service
        .ExecuteAsync(
            command: source,
            cancellationToken: HttpContext.RequestAborted);
}
