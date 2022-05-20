using Tgs.GameProviders.QueryServer.Models;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;

internal class GameRecordHandleService : IGetService<GameRoundHandleQuery, bool>
{
    private readonly IGameRecordMessageSender _messageSender;
    private readonly IGameProvidersQueryService _queryService;

    public GameRecordHandleService(
        IGameProvidersQueryService queryService,
        IGameRecordMessageSender messageSender)
    {
        _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
    }

    public async ValueTask<bool> GetAsync(
        GameRoundHandleQuery query,
        CancellationToken cancellationToken = default)
    {
        var success = true;

        var gameProviderId = query.GameRecord.GameProviderId;
        var playerModel = await _queryService.GetPlayerAsync(query.GameRecord.PlayerCode).ConfigureAwait(false);
        var gameId = await _queryService.GetGameIdAsync(gameProviderId, query.GameRecord.ExternalGameId).ConfigureAwait(false);

        if (playerModel == null || !gameId.HasValue)
        {
            success = false;
        }

        var gameProviderConfigurationId = default(Guid?);
        if (playerModel != null)
        {
            gameProviderConfigurationId = await _queryService
                .GetBrandGetGameProviderConfigurationIdAsync(playerModel.BrandId, gameProviderId)
                .ConfigureAwait(false);

            if (!gameProviderConfigurationId.HasValue)
            {
                success = false;
            }
        }

        var sysDateTimeOffset = DateTimeOffset.UtcNow;
        await _messageSender.SendGameRoundAsync(new GameRoundData
        {
            GameProviderCode = query.GameRecord.GameProviderCode,
            GameProviderId = query.GameRecord.GameProviderId,
            ExternalRoundId = query.GameRecord.ExternalRoundId,
            PlayerCode = playerModel == null ? string.Empty : playerModel.PlayerCode,
            GameId = gameId ?? -1,
            ExternalGameId = query.GameRecord.ExternalGameId,
            CurrencyCode = playerModel == null ? string.Empty : playerModel.DefaultWalletCurrencyCode,
            Status = query.GameRecord.Status,
            GameProviderConfigurationId = gameProviderConfigurationId ?? Guid.Empty,
            WalletId = playerModel?.DefaultWalletId ?? Guid.Empty,
            CreatedOn = sysDateTimeOffset,
            ClosedOn = query.GameRecord.Status == RoundStatus.Closed ? sysDateTimeOffset : null,
            UpdatedOn = sysDateTimeOffset,
            BrandId = playerModel?.BrandId ?? Guid.Empty,
            ExternalCreatedOn = query.GameRecord.ExternalCreatedOn,
            ExternalClosedOn = query.GameRecord.ExternalClosedOn,
            BetAmount = query.GameRecord.BetAmount,
            ValidBetAmount = query.GameRecord.ValidBetAmount,
            PlayerWinLoss = query.GameRecord.PlayerWinLoss,
            Commission = query.GameRecord.Commission,
            Turnover = query.GameRecord.Turnover,
            SettleAmount = query.GameRecord.SettleAmount,
            TieBetAdjustment = null,
            ClientStatus = query.GameRecord.ClientStatus,
            AdditionalData = query.GameRecord.AdditionalData,
            Success = success
        }).ConfigureAwait(false);

        return success;
    }
}
