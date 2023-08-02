using Tgs.GameInterface.PragmaticPlay.Protos;
using Tgs.GameRecord.PragmaticPlay.Core.Models;

namespace Tgs.GameRecord.PragmaticPlay.Core;

public interface IGameRecordUtensil
{
    public ValueTask<RecordInfo> SettlementConvertRecordAsync(
        GameRecordData source,
        CancellationToken cancellationToken = default);
}
