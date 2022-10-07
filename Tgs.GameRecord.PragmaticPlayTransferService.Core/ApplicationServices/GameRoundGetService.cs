using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Tgs.GameRounds.Common.MessageModels;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;

internal class GameRoundGetService : IGetService<GameRoundQuery, bool>
{
    private readonly ILogger<GameRoundGetService> _logger;
    private readonly IMessageSender _messageSender;
    private readonly string _subject;
    private readonly IUtensil _utensil;

    public GameRoundGetService(
        IUtensil utensil,
        IMessageSender messageSender,
        IOptions<GameRecordInfoOptions> options,
        ILogger<GameRoundGetService> logger)
    {
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var gameRecordInfo = options.Value;
        _subject = $"tgs.gp.gamerecord.database.round.{gameRecordInfo.GameProviderCode.ToLower()}";
    }

    public async ValueTask<bool> GetAsync(
        GameRoundQuery query,
        CancellationToken cancellationToken = default)
    {
        var success = true;

        var gameProviderId = query.GameProviderId;
        var playerModel = await _utensil
                    .GetPlayerAsync(
                        query.PlayerCode,
                        cancellationToken)
                    .ConfigureAwait(false);
        var gameId = await _utensil
                    .GetGameIdAsync(
                        gameProviderId,
                        query.ExternalGameId,
                        cancellationToken)
                    .ConfigureAwait(false);

        if (playerModel == null || !gameId.HasValue)
        {
            success = false;
        }

        var gameProviderConfigurationId = default(Guid?);
        if (playerModel != null)
        {
            gameProviderConfigurationId = await _utensil
                .GetBrandGetGameProviderConfigurationIdAsync(
                    playerModel.BrandId,
                    gameProviderId,
                    cancellationToken)
                .ConfigureAwait(false);

            if (!gameProviderConfigurationId.HasValue)
            {
                success = false;
            }
        }

        var sysDateTimeOffset = DateTimeOffset.UtcNow;

        var messageRequest = new GameRecordMessage(

            ExternalRoundId: query.ExternalRoundId,
            GameProviderId: query.GameProviderId,
            GameProviderCode: query.GameProviderCode,
            GameId: gameId ?? -1,
            ExternalGameId: query.ExternalGameId,
            PlayerCode: playerModel == null ? string.Empty : playerModel.PlayerCode,
            CurrencyCode: playerModel == null ? string.Empty : playerModel.DefaultWalletCurrencyCode,
            WalletId: playerModel?.DefaultWalletId ?? Guid.Empty,
            BrandId: playerModel?.BrandId ?? Guid.Empty,
            GameProviderConfigurationId: gameProviderConfigurationId ?? Guid.Empty,
            Status: (GameRounds.Common.GameRoundStatus)query.Status,
            ClientStatus: (GameRounds.Common.GameRoundClientStatus)query.ClientStatus,
            ExternalCreatedOn: query.ExternalCreatedOn,
            ExternalClosedOn: query.ExternalClosedOn,
            BetAmount: query.BetAmount.GetValueOrDefault(0),
            ValidBetAmount: query.ValidBetAmount.GetValueOrDefault(0),
            SettleAmount: query.SettleAmount.GetValueOrDefault(0),
            Commission: query.Commission.GetValueOrDefault(0),
            Turnover: query.Turnover.GetValueOrDefault(0),
            PlayerWinLoss: query.PlayerWinLoss.GetValueOrDefault(0),
            AdditionalData: query.AdditionalData,
            Success: success
            );

        _logger.LogInformation("LogOn:{logOn}  Subject:{subject} MessageRequest:{request}", DateTime.Now.ToString("o"), _subject, messageRequest);

        await _messageSender.PublishAsync(
                _subject,
            messageRequest.ToBson(), cancellationToken:
            cancellationToken)
            .ConfigureAwait(false);

        return success;
    }
}
