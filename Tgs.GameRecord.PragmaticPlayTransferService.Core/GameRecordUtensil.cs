using Tgs.GameInterface.PragmaticPlay.Protos;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.Models;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

internal class GameRecordUtensil : IGameRecordUtensil
{
    private readonly IUtensil _utensil;

    public GameRecordUtensil(
        IUtensil utensil)
    {
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
    }

    public async ValueTask<RecordInfo> SettlementConvertRecordAsync(
        GameRecordData source,
        CancellationToken cancellationToken = default)
    {
        var gameProviderId = await _utensil
            .GetGameProviderIdAsync(
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var gameProviderCode = _utensil
            .GetGameProviderCode();

        bool success = true;

        var player = await _utensil
            .GetPlayerAsync(
                playerCode: source.PlayerCode,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var gameId = await _utensil
            .GetGameIdAsync(
                gameProviderId: gameProviderId,
                gameExternalId: source.ExternalGameId,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (player == null || !gameId.HasValue)
        {
            success = false;
        }

        Guid? gameProviderConfigurationId = default;
        if (player != null)
        {
            gameProviderConfigurationId = await _utensil
                .GetBrandGameProviderConfigurationIdAsync(
                    brandId: player.BrandId,
                    gameProviderId: gameProviderId,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (!gameProviderConfigurationId.HasValue)
            {
                success = false;
            }
        }

        var externalCreatedOn = source.ExternalCreatedOn.ToDateTimeOffset();
        DateTimeOffset? externalClosedOn = source.IsClosed ? source.ExternalClosedOn.ToDateTimeOffset() : null;
        var bet = source.Bet.ToDecimal().GetValueOrDefault(0);
        var validBet = source.Bet.ToDecimal().GetValueOrDefault(0);
        var settle = source.Settle.ToDecimal().GetValueOrDefault(0);
        var commission = 0;
        var turnover = source.Bet.ToDecimal().GetValueOrDefault(0);
        var winLoss = source.WinLoss.ToDecimal().GetValueOrDefault(0);

        return new RecordInfo(
            Success: success,
            GameProviderId: gameProviderId,
            GameProviderCode: gameProviderCode,
            ExternalRoundId: source.ExternalRoundId,
            GameId: gameId ?? -1,
            ExternalGameId: source.ExternalGameId.ToString(),
            PlayerCode: (player == null) ? string.Empty : player.PlayerCode,
            WalletId: (player == null) ? Guid.Empty : player.DefaultWalletId,
            CurrencyCode: (player == null) ? string.Empty : player.DefaultWalletCurrencyCode,
            BrandId: (player == null) ? Guid.Empty : player.BrandId,
            GameProviderConfigurationId: gameProviderConfigurationId ?? Guid.Empty,
            Closed: source.IsClosed,
            ExternalCreatedOn: externalCreatedOn,
            ExternalClosedOn: externalClosedOn,
            Bet: bet,
            ValidBet: validBet,
            Settle: settle,
            Commission: commission,
            Turnover: turnover,
            WinLoss: winLoss,
            AdditionalData: source.AdditionalData);
    }
}
