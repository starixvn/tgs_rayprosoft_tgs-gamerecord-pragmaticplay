using Microsoft.Extensions.Options;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlay.Core.ApplicationServices;

internal class HandleRecordCommandService : ICommandService<HandleRecordCommand>
{
    private readonly IGameRecordHandleClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfo;

    public HandleRecordCommandService(
        IOptions<GameRecordInfoOptions> options,
        IGameRecordHandleClient client)
    {
        _gameRecordInfo = options.Value;
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
    {
        var externalCreatedOn = command.Record.ExternalCreatedOn.ToDateTimeOffset();
        var externalClosedOn = (DateTimeOffset?)null;
        var bet = command.Record.Bet.ToDecimal().GetValueOrDefault(0);
        var validBet = command.Record.Bet.ToDecimal().GetValueOrDefault(0);
        var settle = 0.0m;
        var commission = 0.0m;
        var turnover = 0.0m;
        var playerWinLoss = 0.0m;

        var status = GameRecordStatus.Open;
        var clientStatus = GameRecordClientStatus.Open;
        if (command.Record.IsClosed)
        {
            status = GameRecordStatus.Closed;
            externalClosedOn = command.Record.ExternalClosedOn.ToDateTimeOffset();
            settle = command.Record.Settle.ToDecimal().GetValueOrDefault(0);
            turnover = command.Record.Bet.ToDecimal().GetValueOrDefault(0);
            playerWinLoss = command.Record.WinLoss.ToDecimal().GetValueOrDefault(0);
            clientStatus = playerWinLoss <= 0 ? GameRecordClientStatus.Loss : GameRecordClientStatus.Win;
        }

        await _client
        .MessageAsync(
            gameProviderCode: _gameRecordInfo.GameProviderCode,
            externalRoundId: command.Record.ExternalRoundId,
            externalGameId: command.Record.ExternalGameId,
            playerCode: command.Record.PlayerCode,
            status: status,
            clientStatus: clientStatus,
            externalCreatedOn: externalCreatedOn,
            externalClosedOn: externalClosedOn,
            bet: bet,
            validBet: validBet,
            settle: settle,
            commission: commission,
            turnover: turnover,
            playerWinLoss: playerWinLoss,
            additionalData: command.Record.AdditionalData,
            cancellationToken: cancellationToken)
        .ConfigureAwait(false);
    }
}
