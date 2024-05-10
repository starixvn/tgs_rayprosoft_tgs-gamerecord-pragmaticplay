using System.Text.Json.Nodes;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameInterface.PragmaticPlay.Protos;
using Tgs.GameRecord.PragmaticPlay.Core.ApplicationServices;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.PragmaticPlay.Core.UnitTest.ApplicationServices;

public class HandleRecordCommandServiceUnitTest
{
	private static readonly string[] _GameDataTypes = ["RNG", "LC", "VSB"];

	[Fact]
	public async Task Loss()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
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
		var fakeMessageSender = Substitute.For<IMessageSender>();

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
			faceLogger,
			gameRecordInfo,
			fakeMessageSender);

		await service.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(1)
			.PublishAsync(
			subject: Arg.Is(Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest),
			data: Arg.Is(new Handle.Common.Protos.GameRecordHandleRequest
			{
				GameProviderCode = gameRecordInfo.Value.GameProviderCode,
				PlayerCode = command.Record.PlayerCode,
				ExternalRoundId = command.Record.ExternalRoundId,
				ExternalBetId = command.Record.ExternalRoundId,
				ExternalGameId = command.Record.ExternalGameId,
				ExternalCreatedOn = command.Record.ExternalCreatedOn,
				ExternalClosedOn = command.Record.ExternalClosedOn,
				ExternalPayoffOn = command.Record.ExternalClosedOn,
				ExternalResettleOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				BetState = Handle.Common.Protos.BetState.Loss,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 1m,
				ValidBet = 1m,
				Settle = 0.0M,
				Commission = 0.0M,
				Turnover = 1m,
				WinLoss = -1m,
				Jackpot = 0.0M,
				AdditionalData = command.Record.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Win()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
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
		var fakeMessageSender = Substitute.For<IMessageSender>();

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
			faceLogger,
			gameRecordInfo,
			fakeMessageSender);

		await service.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(1)
			.PublishAsync(
			subject: Arg.Is(Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest),
			data: Arg.Is(new Handle.Common.Protos.GameRecordHandleRequest
			{
				GameProviderCode = gameRecordInfo.Value.GameProviderCode,
				PlayerCode = command.Record.PlayerCode,
				ExternalRoundId = command.Record.ExternalRoundId,
				ExternalBetId = command.Record.ExternalRoundId,
				ExternalGameId = command.Record.ExternalGameId,
				ExternalCreatedOn = command.Record.ExternalCreatedOn,
				ExternalClosedOn = command.Record.ExternalClosedOn,
				ExternalPayoffOn = command.Record.ExternalClosedOn,
				ExternalResettleOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				BetState = Handle.Common.Protos.BetState.Win,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 100m,
				ValidBet = 100m,
				Settle = 200m,
				Commission = 0.0M,
				Turnover = 100m,
				WinLoss = 100m,
				Jackpot = 0.0M,
				AdditionalData = command.Record.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Running()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
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
		var fakeMessageSender = Substitute.For<IMessageSender>();

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
			faceLogger,
			gameRecordInfo,
			fakeMessageSender);

		await service.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(1)
			.PublishAsync(
			subject: Arg.Is(Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest),
			data: Arg.Is(new Handle.Common.Protos.GameRecordHandleRequest
			{
				GameProviderCode = gameRecordInfo.Value.GameProviderCode,
				PlayerCode = command.Record.PlayerCode,
				ExternalRoundId = command.Record.ExternalRoundId,
				ExternalBetId = command.Record.ExternalRoundId,
				ExternalGameId = command.Record.ExternalGameId,
				ExternalCreatedOn = command.Record.ExternalCreatedOn,
				ExternalClosedOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				ExternalPayoffOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				ExternalResettleOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				BetState = Handle.Common.Protos.BetState.Running,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 100m,
				ValidBet = 100m,
				Settle = 0.0M,
				Commission = 0.0M,
				Turnover = 0.0M,
				WinLoss = 0.0M,
				Jackpot = 0.0M,
				AdditionalData = command.Record.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}
}
