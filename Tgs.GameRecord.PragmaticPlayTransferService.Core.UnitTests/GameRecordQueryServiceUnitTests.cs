using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.GameProviders.Client;
using Tgs.GameProviders.Domains;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.UnitTests;

internal class GameRecordQueryServiceUnitTests
{
    [Test]
    public async Task Normal_Process()
    {
        var fakeGameRecordListClient = Substitute.For<IGameRecordListClient>();
        var fakeHandleService = Substitute.For<IGetService<GameRoundHandleQuery, bool>>();

        var externalClosedOn = DateTimeOffset.UtcNow;
        var nextTime = DateTimeOffset.UtcNow;

        var gameRecordResponse = new PragmaticPlayTransferGameRecordResponse
        {
            Datas = new GameRecordModle[]
            {
                new GameRecordModle
                {
                    GameProviderCode = GameProviderType.PragmaticPlayTransfer,
                    GameProviderId = Guid.Parse("e4ecfa98-b373-4123-87e0-cb9e883d0691"),
                    PlayerCode = "4ucmqplxb029",
                    BetAmount = 10m,
                    SettleAmount = 20m,
                    ValidBetAmount = 10m,
                    Turnover = 10m,
                    PlayerWinLoss = 10m,
                    ExternalRoundId = "38660",
                    ExternalCreatedOn = externalClosedOn.AddSeconds(-5),
                    ExternalClosedOn = externalClosedOn,
                    ExternalGameId = "vpmr9",
                    Status = GameRecordRoundStatus.Closed,
                    ClientStatus = GameRecordRoundClientStatus.Win,
                    TieBetAdjustment = null,
                    Commission = null,
                    AdditionalData = string.Empty
                }
            },
            NextTime = nextTime,
            Success = true
        };

        var expected = new GameRecordResult(
            true,
            1,
            1,
            0,
            externalClosedOn,
            nextTime);

        var query = new GameRecordQuery(
            PragmaticPlayTransferGameDataType.RNG,
            DateTimeOffset.Now.AddMinutes(-10));

        _ = fakeGameRecordListClient.PragmaticPlayTransferGameRecordListAsync(
                Arg.Is(query.DataType),
                Arg.Is(query.StartTime))
            .Returns(gameRecordResponse);

        _ = fakeHandleService.GetAsync(Arg.Any<GameRoundHandleQuery>())
            .Returns(true);

        var sut = new GameRecordQueryService(fakeGameRecordListClient, fakeHandleService);

        var actual = await sut.GetAsync(query).ConfigureAwait(false);

        Assert.AreEqual(expected, actual);
    }
}
