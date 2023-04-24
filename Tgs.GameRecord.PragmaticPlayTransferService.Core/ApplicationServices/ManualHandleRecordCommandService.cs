using System.Text.Json;
using Tgs.GameRounds.Work.Manual.Client;
using Tgs.GameRounds.Work.Manual.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;

internal class ManualHandleRecordCommandService : ICommandService<ManualHandleRecordCommand>
{
    private readonly IGameRoundsWorkManualClient _client;

    public ManualHandleRecordCommandService(
        IGameRoundsWorkManualClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask ExecuteAsync(ManualHandleRecordCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Record.Success)
        {
            var status = RoundStatus.Open;
            var clientStatus = RoundClientStatus.Open;
            if (command.Record.Closed)
            {
                status = RoundStatus.Closed;
                clientStatus = command.Record.WinLoss <= 0 ? RoundClientStatus.Loss : RoundClientStatus.Win;
            }

            await _client
            .RoundBidding(
                gameProviderId: command.Record.GameProviderId,
                gameProviderCode: command.Record.GameProviderCode,
                externalRoundId: command.Record.ExternalRoundId,
                gameId: command.Record.GameId,
                externalGameId: command.Record.ExternalGameId,
                playerCode: command.Record.PlayerCode,
                walletId: command.Record.WalletId,
                currencyCode: command.Record.CurrencyCode,
                brandId: command.Record.BrandId,
                gameProviderConfigurationId: command.Record.GameProviderConfigurationId,
                status: status,
                clientStatus: clientStatus,
                externalCreatedOn: command.Record.ExternalCreatedOn,
                externalClosedOn: command.Record.ExternalClosedOn,
                bet: command.Record.Bet,
                validBet: command.Record.ValidBet,
                settle: command.Record.Settle,
                commission: command.Record.Commission,
                turnover: command.Record.Turnover,
                playerWinLoss: command.Record.WinLoss,
                additionalData: command.Record.AdditionalData,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        }
        else
        {
            await _client
            .RoundMissing(
                gameProviderId: command.Record.GameProviderId,
                gameProviderCode: command.Record.GameProviderCode,
                externalRoundId: command.Record.ExternalRoundId,
                gameId: command.Record.GameId,
                externalGameId: command.Record.ExternalGameId,
                playerCode: command.Record.PlayerCode,
                data: JsonSerializer.Serialize(command.Record),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        }
    }
}
