using System.Text.Json.Nodes;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameInterface.PragmaticPlay.Protos;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;
using Tgs.GameRecord.PragmaticPlay.Core.ApplicationServices;

namespace Tgs.GameRecord.PragmaticPlay.Core.UnitTest.ApplicationServices;

public class HandleRecordCommandServiceUnitTest
{
    private static readonly string[] _GameDataTypes = ["RNG", "LC", "VSB"];

    [Fact]
    public async void Normal_Process_Settlement_Loss()
    {
        var gameRecordInfo = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "PPTF",
            GameProviderConfiguration = "PPTFConfiguration",
            HangfirePrefix = "gamerecord-pp-hangfire:",
            HangfireLocking = "gamerecord-pp-locking:",
            GameDataTypes = _GameDataTypes,
            IntervalMinutes = 1,
            IntervalMinutesMax = 10,
            IntervalMinutesMinimum = 1,
            VirtualSportsIntervalMinutes = 10,
            LiveDealerIntervalMinutes = 5,
            LockMinute = 8
        });
        var fakeClient = Substitute.For<IGameRecordHandleClient>();

        var command = new HandleRecordCommand(
            Record: new GameRecordData
            {
                ExternalRoundId = "97419402008",
                ExternalGameId = "439",
                PlayerCode = "4ucmqplxb029",
                IsClosed = true,
                ExternalCreatedOn = DateTimeOffset.Parse("2023-04-21T03:24:14+00:00").ToTimestamp(),
                ExternalClosedOn = DateTimeOffset.Parse("2023-04-21T03:24:38+00:00").ToTimestamp(),
                Bet = 1m,
                Settle = 0,
                WinLoss = -1m,
                AdditionalData = "{\"result\":\"Player\",\"bets\":[{\"c\":\"Plyr(NoComm)\",\"a\":\"1\"}]}"
            });

        var service = new HandleRecordCommandService(
            gameRecordInfo,
            fakeClient);

        await service.ExecuteAsync(command);

        await fakeClient
            .Received(1)
            .MessageAsync(
            gameProviderCode: Arg.Is(gameRecordInfo.Value.GameProviderCode),
            playerCode: Arg.Is(command.Record.PlayerCode),
            wagerState: Arg.Is(WagerStates.Lose),
            wagerType: Arg.Is(WagerTypes.Normal),
            freeGameState: Arg.Is(FreeGameStates.None),
            gambleState: Arg.Is(GambleStates.None),
            cashoutState: Arg.Is(CashoutStates.None),
            resettleState: Arg.Is(ResettleStates.None),
            externalRoundId: Arg.Is(command.Record.ExternalRoundId),
            externalWagerId: Arg.Is(command.Record.ExternalRoundId),
            externalGameId: Arg.Is(command.Record.ExternalGameId),
            externalCreatedOn: Arg.Is(command.Record.ExternalCreatedOn.ToDateTimeOffset()),
            externalClosedOn: Arg.Is(command.Record.ExternalClosedOn.ToDateTimeOffset()),
            externalPayoffOn: Arg.Is(command.Record.ExternalClosedOn.ToDateTimeOffset()),
            externalResettleOn: Arg.Is(DateTimeOffset.UnixEpoch),
            bet: Arg.Is(command.Record.Bet.ToDecimal().GetValueOrDefault(0)),
            validBet: Arg.Is(command.Record.Bet.ToDecimal().GetValueOrDefault(0)),
            settle: Arg.Is(command.Record.Settle.ToDecimal().GetValueOrDefault(0)),
            commission: Arg.Is(0.0m),
            turnover: Arg.Is(command.Record.Bet.ToDecimal().GetValueOrDefault(0)),
            winLoss: Arg.Is(command.Record.WinLoss.ToDecimal().GetValueOrDefault(0)),
            additionalData: Arg.Any<JsonNode>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async void Normal_Process_Settlement_Win()
    {
        var gameRecordInfo = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "PPTF",
            GameProviderConfiguration = "PPTFConfiguration",
            HangfirePrefix = "gamerecord-pp-hangfire:",
            HangfireLocking = "gamerecord-pp-locking:",
            GameDataTypes = _GameDataTypes,
            IntervalMinutes = 1,
            IntervalMinutesMax = 10,
            IntervalMinutesMinimum = 1,
            VirtualSportsIntervalMinutes = 10,
            LiveDealerIntervalMinutes = 5,
            LockMinute = 8
        });
        var fakeClient = Substitute.For<IGameRecordHandleClient>();

        var command = new HandleRecordCommand(
            Record: new GameRecordData
            {
                ExternalRoundId = "643583c37c18c573b57bbb2c",
                ExternalGameId = "22020",
                PlayerCode = "4uispltzj9d8",
                IsClosed = true,
                ExternalCreatedOn = DateTimeOffset.Parse("2023-04-10 23:45:22+08:00").ToTimestamp(),
                ExternalClosedOn = DateTimeOffset.Parse("2023-04-10 23:45:52+08:00").ToTimestamp(),
                Bet = 100m,
                Settle = 200m,
                WinLoss = 100m,
                AdditionalData = "{\"recordID\": \"643583c37c18c573b57bbb2c\"}"
            });

        var service = new HandleRecordCommandService(
            gameRecordInfo,
            fakeClient);

        await service.ExecuteAsync(command);

        await fakeClient
            .Received(1)
            .MessageAsync(
            gameProviderCode: Arg.Is(gameRecordInfo.Value.GameProviderCode),
            playerCode: Arg.Is(command.Record.PlayerCode),
            wagerState: Arg.Is(WagerStates.Win),
            wagerType: Arg.Is(WagerTypes.Normal),
            freeGameState: Arg.Is(FreeGameStates.None),
            gambleState: Arg.Is(GambleStates.None),
            cashoutState: Arg.Is(CashoutStates.None),
            resettleState: Arg.Is(ResettleStates.None),
            externalRoundId: Arg.Is(command.Record.ExternalRoundId),
            externalWagerId: Arg.Is(command.Record.ExternalRoundId),
            externalGameId: Arg.Is(command.Record.ExternalGameId),
            externalCreatedOn: Arg.Is(command.Record.ExternalCreatedOn.ToDateTimeOffset()),
            externalClosedOn: Arg.Is(command.Record.ExternalClosedOn.ToDateTimeOffset()),
            externalPayoffOn: Arg.Is(command.Record.ExternalClosedOn.ToDateTimeOffset()),
            externalResettleOn: Arg.Is(DateTimeOffset.UnixEpoch),
            bet: Arg.Is(command.Record.Bet.ToDecimal().GetValueOrDefault(0)),
            validBet: Arg.Is(command.Record.Bet.ToDecimal().GetValueOrDefault(0)),
            settle: Arg.Is(command.Record.Settle.ToDecimal().GetValueOrDefault(0)),
            commission: Arg.Is(0.0m),
            turnover: Arg.Is(command.Record.Bet.ToDecimal().GetValueOrDefault(0)),
            winLoss: Arg.Is(command.Record.WinLoss.ToDecimal().GetValueOrDefault(0)),
            additionalData: Arg.Any<JsonNode>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async void Normal_Process_Unsettlement()
    {
        var gameRecordInfo = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "PPTF",
            GameProviderConfiguration = "PPTFConfiguration",
            HangfirePrefix = "gamerecord-pp-hangfire:",
            HangfireLocking = "gamerecord-pp-locking:",
            GameDataTypes = _GameDataTypes,
            IntervalMinutes = 1,
            IntervalMinutesMax = 10,
            IntervalMinutesMinimum = 1,
            VirtualSportsIntervalMinutes = 10,
            LiveDealerIntervalMinutes = 5,
            LockMinute = 8
        });
        var fakeClient = Substitute.For<IGameRecordHandleClient>();

        var command = new HandleRecordCommand(
            Record: new GameRecordData
            {
                ExternalRoundId = "643583c37c18c573b57bbb2c",
                ExternalGameId = "22020",
                PlayerCode = "4uispltzj9d8",
                IsClosed = false,
                ExternalCreatedOn = DateTimeOffset.Parse("2023-04-10 23:45:22+08:00").ToTimestamp(),
                ExternalClosedOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
                Bet = 100m,
                Settle = 0m,
                WinLoss = 0m,
                AdditionalData = "{\"recordID\": \"643583c37c18c573b57bbb2c\"}"
            });

        var service = new HandleRecordCommandService(
            gameRecordInfo,
            fakeClient);

        await service.ExecuteAsync(command);

        await fakeClient
            .Received(1)
            .MessageAsync(
            gameProviderCode: Arg.Is(gameRecordInfo.Value.GameProviderCode),
            playerCode: Arg.Is(command.Record.PlayerCode),
            wagerState: Arg.Is(WagerStates.Running),
            wagerType: Arg.Is(WagerTypes.Normal),
            freeGameState: Arg.Is(FreeGameStates.None),
            gambleState: Arg.Is(GambleStates.None),
            cashoutState: Arg.Is(CashoutStates.None),
            resettleState: Arg.Is(ResettleStates.None),
            externalRoundId: Arg.Is(command.Record.ExternalRoundId),
            externalWagerId: Arg.Is(command.Record.ExternalRoundId),
            externalGameId: Arg.Is(command.Record.ExternalGameId),
            externalCreatedOn: Arg.Is(command.Record.ExternalCreatedOn.ToDateTimeOffset()),
            externalClosedOn: Arg.Is(DateTimeOffset.UnixEpoch),
            externalPayoffOn: Arg.Is(DateTimeOffset.UnixEpoch),
            externalResettleOn: Arg.Is(DateTimeOffset.UnixEpoch),
            bet: Arg.Is(command.Record.Bet.ToDecimal().GetValueOrDefault(0)),
            validBet: Arg.Is(command.Record.Bet.ToDecimal().GetValueOrDefault(0)),
            settle: Arg.Is(0.0m),
            commission: Arg.Is(0.0m),
            turnover: Arg.Is(0.0m),
            winLoss: Arg.Is(0.0m),
            additionalData: Arg.Any<JsonNode>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }
}
