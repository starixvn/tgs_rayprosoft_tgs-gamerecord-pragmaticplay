using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Tgs.GameProviders.QueryServer.Models;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;

internal class GameRecordHandleService : IGetService<GameRoundHandleQuery, bool>
{
    private readonly ILogger<GameRecordHandleService> _logger;
    private readonly IMessageSender _messageSender;
    private readonly string _subject;
    private readonly IUtensil _utensil;

    public GameRecordHandleService(
        IUtensil utensil,
        IMessageSender messageSender,
        IOptions<GameRecordInfoOptions> options,
        ILogger<GameRecordHandleService> logger)
    {
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var gameRecordInfo = options.Value;
        _subject = $"tgs.gp.gamerecord.database.round.{gameRecordInfo.GameProviderCode.ToLower()}";
    }

    public async ValueTask<bool> GetAsync(
        GameRoundHandleQuery query,
        CancellationToken cancellationToken = default)
    {
        var success = true;

        var gameProviderId = query.GameRecord.GameProviderId;
        var playerModel = await _utensil.GetPlayerAsync(query.GameRecord.PlayerCode).ConfigureAwait(false);
        var gameId = await _utensil.GetGameIdAsync(gameProviderId, query.GameRecord.ExternalGameId).ConfigureAwait(false);

        if (playerModel == null || !gameId.HasValue)
        {
            success = false;
        }

        var gameProviderConfigurationId = default(Guid?);
        if (playerModel != null)
        {
            gameProviderConfigurationId = await _utensil
                .GetBrandGetGameProviderConfigurationIdAsync(playerModel.BrandId, gameProviderId)
                .ConfigureAwait(false);

            if (!gameProviderConfigurationId.HasValue)
            {
                success = false;
            }
        }

        var sysDateTimeOffset = DateTimeOffset.UtcNow;

        var messageRequest = new GameRoundMessageRequest
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
        };

        _logger.LogInformation("LogOn:{logOn}  Subject:{subject} MessageRequest:{request}", DateTime.Now.ToString("o"), _subject, messageRequest);

        await _messageSender.PublishAsync(
                _subject,
            messageRequest.ToBson(), cancellationToken:
            cancellationToken)
            .ConfigureAwait(false);

        return success;
    }
}
