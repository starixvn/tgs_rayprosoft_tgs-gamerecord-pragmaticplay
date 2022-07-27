using System.Text.Json;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.DispatchServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.MessageHandler;

public class ManualMessageHandler : IMessageProcessor
{
    private readonly IGetService<ManualDispatchQuery, ManualDispatchResult> _service;

    public ManualMessageHandler(IGetService<ManualDispatchQuery, ManualDispatchResult> service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default)
    {
        var request = JsonSerializer.Deserialize<ManualMessageRequest>(data.Span)!;

        var query = new ManualDispatchQuery(
            GameDataType: request.GameDataType,
            StartTime: request.StartTime,
            EndTime: request.EndTime);

        var result = await _service.GetAsync(
            query: query,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var response = new ManualMessageResponse(
            Success: result.Success,
            Message: result.Message,
            NextTime: result.NextTime,
            LastGameRecordTime: result.LastGameRecordTime);

        return JsonSerializer.SerializeToUtf8Bytes(response);
    }
}
