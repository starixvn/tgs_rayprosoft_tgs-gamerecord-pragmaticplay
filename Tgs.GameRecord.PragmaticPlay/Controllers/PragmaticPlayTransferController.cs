using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.PragmaticPlay.Common;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.PragmaticPlay.Controllers;

[Route("[controller]")]
[ApiController]
public class PragmaticPlayTransferController : ControllerBase
{
    /// <summary>
    /// Games the record manual asynchronous.
    /// </summary>
    /// <param name="messageSender">The message sender.</param>
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
    public ValueTask GameRecordManualAsync(
        [FromServices] IMessageSender messageSender,
        PragmaticPlayGameRecordManualMessage source)
        => messageSender.PublishAsync(
            subject: PragmaticPlayNatsConst.ManualPublish,
            data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(source)),
            cancellationToken: HttpContext.RequestAborted);

    /// <summary>
    /// Games the record schedule asynchronous.
    /// </summary>
    /// <param name="messageSender">The message sender.</param>
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
    public ValueTask GameRecordScheduleAsync(
        [FromServices] IMessageSender messageSender,
        PragmaticPlayGameRecordScheduleMessage source)
        => messageSender.PublishAsync(
            subject: PragmaticPlayNatsConst.SchedulePublish,
            data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(source)),
            cancellationToken: HttpContext.RequestAborted);
}
