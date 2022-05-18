using Microsoft.Extensions.Logging;
using Tgs.GameProviders.Domains;
using Tgs.GameProviders.QueryServer.Models;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;

internal class GameRecordHandleService : IGetService<GameRoundHandleQuery, bool>
{
    private readonly IGameProviderService _gameProviderService;
    private readonly IGameService _gameService;

    private readonly IGameRecordMessageSender _messageSender;
    private readonly IPlayerService _playerService;

    public GameRecordHandleService(
        IGameProviderService gameProviderService,
        IGameService gameService,
        IPlayerService playerService,
        IGameRecordMessageSender messageSender)
    {
        _gameProviderService = gameProviderService ?? throw new ArgumentNullException(nameof(gameProviderService));
        _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
        _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
    }

    public async ValueTask<bool> GetAsync(
        GameRoundHandleQuery query,
        CancellationToken cancellationToken = default)
    {
        var gameProviderCode = GameProviderType.PragmaticPlayTransfer;

        var success = true;

        var gameProviderId = await _gameProviderService.GetGameProviderIdAsync(gameProviderCode).ConfigureAwait(false);
        var playerInfo = await _playerService.GetAsync(gameProviderId, query.GameRecord.PlayerCode).ConfigureAwait(false);
        var gameId = await _gameService.GetGameIdAsync(gameProviderId, query.GameRecord.ExternalGameId).ConfigureAwait(false);

        if (playerInfo == null || !gameId.HasValue)
        {
            success = false;
        }

        var sysDateTimeOffset = DateTimeOffset.UtcNow;
        await _messageSender.SendGameRoundAsync(new GameRoundData
        {
            GameProviderCode = query.GameRecord.GameProviderCode,
            GameProviderId = query.GameRecord.GameProviderId,
            ExternalRoundId = query.GameRecord.ExternalRoundId,
            PlayerCode = playerInfo == null ? string.Empty : playerInfo.PlayerCode,
            GameId = gameId ?? -1,
            ExternalGameId = query.GameRecord.ExternalGameId,
            CurrencyCode = playerInfo == null ? string.Empty : playerInfo.DefaultWalletCurrencyCode,
            Status = query.GameRecord.Status,
            GameProviderConfigurationId = playerInfo?.GameProviderConfigurationId ?? Guid.Empty,
            WalletId = playerInfo?.DefaultWalletId ?? Guid.Empty,
            CreatedOn = sysDateTimeOffset,
            ClosedOn = query.GameRecord.Status == RoundStatus.Closed ? sysDateTimeOffset : null,
            UpdatedOn = sysDateTimeOffset,
            BrandId = playerInfo?.BrandId ?? Guid.Empty,
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
