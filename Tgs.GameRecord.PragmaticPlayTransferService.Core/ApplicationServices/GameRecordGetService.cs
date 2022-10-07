using Tgs.GameProviders.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;

internal class GameRecordGetService : IGetService<GameRecordQuery, GameRecordResult>
{
    private readonly IGameRecordListClient _gameRecordListClient;
    private readonly IGetService<GameRoundQuery, bool> _handleService;

    public GameRecordGetService(
        IGameRecordListClient gameRecordListClient,
        IGetService<GameRoundQuery, bool> handleService)
    {
        _gameRecordListClient = gameRecordListClient ?? throw new ArgumentNullException(nameof(gameRecordListClient));
        _handleService = handleService;
    }

    public async ValueTask<GameRecordResult> GetAsync(
        GameRecordQuery query,
        CancellationToken cancellationToken = default)
    {
        var count = 0;
        var successCount = 0;
        var failureCount = 0;
        var lastRecordTime = default(DateTimeOffset?);
        var nextTime = default(DateTimeOffset?);

        var response = await _gameRecordListClient.PragmaticPlayTransferGameRecordListAsync(
            dataType: query.DataType,
            startTime: query.StartTime,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (response == null || !response.Success)
        {
            return new GameRecordResult(
                false,
                count,
                successCount,
                failureCount,
                lastRecordTime,
                nextTime);
        }

        nextTime = response.NextTime;

        var records = response.Datas ?? Array.Empty<GameRecordModle>();

        foreach (var data in records)
        {
            count++;

            var gameRound = new GameRoundQuery(
                data.GameProviderCode,
                data.GameProviderId,
                data.PlayerCode,
                data.Status,
                data.ClientStatus,
                data.ExternalRoundId,
                data.ExternalGameId,
                data.ExternalCreatedOn!.Value,
                data.ExternalClosedOn,
                data.BetAmount,
                data.ValidBetAmount,
                data.PlayerWinLoss,
                data.Commission,
                data.Turnover,
                data.SettleAmount,
                data.TieBetAdjustment,
                data.AdditionalData);

            var isSuccess = await _handleService
                .GetAsync(
                    gameRound,
                    cancellationToken)
                .ConfigureAwait(false);

            if (isSuccess)
                successCount++;
            else
                failureCount++;

            lastRecordTime = data.ExternalClosedOn ?? data.ExternalCreatedOn;
        }

        return new GameRecordResult(
            true,
            count,
            successCount,
            failureCount,
            lastRecordTime,
            nextTime);
    }
}
