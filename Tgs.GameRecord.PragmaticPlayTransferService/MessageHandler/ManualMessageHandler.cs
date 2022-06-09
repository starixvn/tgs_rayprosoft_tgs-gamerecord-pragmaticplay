using System.Text.Json;
using Tgs.GameRecord.PragmaticPlayTransferService.Core;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.MessageHandler;

public class ManualMessageHandler : IMessageProcessor
{
    private readonly IGetService<GameRecordQuery, GameRecordResult> _getService;

    public ManualMessageHandler(
        IGetService<GameRecordQuery, GameRecordResult> getService)
    {
        _getService = getService ?? throw new ArgumentNullException(nameof(getService));
    }

    public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default)
    {
        var request = JsonSerializer.Deserialize<ManualMessageRequest>(data.Span)!;

        var result = await _getService.GetAsync(
            new GameRecordQuery(
                request.DataType,
                request.StartTime),
            cancellationToken).ConfigureAwait(false);

        var response = new ManualMessageResponse(
            result.Success,
            result.Count,
            result.SuccessCount,
            result.FailureCount,
            result.LastGameRecordTime,
            result.NextTime);

        return JsonSerializer.SerializeToUtf8Bytes(response);
    }
}
