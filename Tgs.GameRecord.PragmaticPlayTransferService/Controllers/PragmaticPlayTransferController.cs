using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Controllers;

[Route("[controller]")]
[ApiController]
public class DreamGameController : ControllerBase
{
    [HttpPost("manual")]
    public ValueTask GameRecordAsync(
       [FromServices] ICommandService<ManualPullRecordCommand> commandService,
       [FromBody] ManualPullRecordCommand source)
        => commandService
        .ExecuteAsync(
            command: source,
            cancellationToken: HttpContext.RequestAborted);

    [HttpPost("schedule")]
    public ValueTask GameRecordAsync(
       [FromServices] ICommandService<SchedulePullRecordCommand> commandService,
       [FromBody] SchedulePullRecordCommand source)
        => commandService
        .ExecuteAsync(
            command: source,
            cancellationToken: HttpContext.RequestAborted);
}
