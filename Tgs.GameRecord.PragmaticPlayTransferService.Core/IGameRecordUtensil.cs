using Tgs.GameInterface.PragmaticPlay.Protos;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.Models;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public interface IGameRecordUtensil
{
    public ValueTask<RecordInfo> SettlementConvertRecordAsync(
        GameRecordData source,
        CancellationToken cancellationToken = default);
}
