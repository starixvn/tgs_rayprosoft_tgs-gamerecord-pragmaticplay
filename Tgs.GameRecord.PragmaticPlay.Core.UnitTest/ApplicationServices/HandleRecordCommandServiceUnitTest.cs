using System.Text.Json;
using NSubstitute;
using Tgs.GameRecord.PragmaticPlay.Core.ApplicationServices;
using Tgs.GameRecord.PragmaticPlay.Core.Models;
using Tgs.GameRounds.Work.Client;
using Tgs.GameRounds.Work.Common.Protos;

namespace Tgs.GameRecord.PragmaticPlay.Core.UnitTest.ApplicationServices;

public class HandleRecordCommandServiceUnitTest
{
    [Fact]
    public async void Normal_Process_Settlement_Loss()
    {
        var fakeClient = Substitute.For<IGameRoundsWorkClient>();

        var command = new HandleRecordCommand(
            Record: new RecordInfo(
                Success: true,
                GameProviderId: Guid.NewGuid(),
                GameProviderCode: "PPTF",
                ExternalRoundId: "643583c37c18c573b57bbb2c",
                GameId: 1,
                ExternalGameId: "22020",
                PlayerCode: "4uispltzj9d8",
                WalletId: Guid.NewGuid(),
                CurrencyCode: "PHP",
                BrandId: Guid.NewGuid(),
                GameProviderConfigurationId: Guid.NewGuid(),
                Closed: true,
                ExternalCreatedOn: DateTimeOffset.Parse("2023-04-10 23:45:22+08:00"),
                ExternalClosedOn: DateTimeOffset.Parse("2023-04-10 23:45:52+08:00"),
                Bet: 100,
                ValidBet: 100,
                Settle: 0,
                Commission: 0,
                Turnover: 100,
                WinLoss: -100,
                AdditionalData: "{\"recordID\": \"643583c37c18c573b57bbb2c\"}"
                ));

        var service = new HandleRecordCommandService(fakeClient);

        await service
            .ExecuteAsync(
                command)
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .RoundBidding(
                gameProviderId: Arg.Is(command.Record.GameProviderId),
                gameProviderCode: Arg.Is(command.Record.GameProviderCode),
                externalRoundId: Arg.Is(command.Record.ExternalRoundId),
                gameId: Arg.Is(command.Record.GameId),
                externalGameId: Arg.Is(command.Record.ExternalGameId),
                playerCode: Arg.Is(command.Record.PlayerCode),
                walletId: Arg.Is(command.Record.WalletId),
                currencyCode: Arg.Is(command.Record.CurrencyCode),
                brandId: Arg.Is(command.Record.BrandId),
                gameProviderConfigurationId: Arg.Is(command.Record.GameProviderConfigurationId),
                status: Arg.Is(RoundStatus.Closed),
                clientStatus: Arg.Is(RoundClientStatus.Loss),
                externalCreatedOn: Arg.Is(command.Record.ExternalCreatedOn),
                externalClosedOn: Arg.Is(command.Record.ExternalClosedOn),
                bet: Arg.Is(command.Record.Bet),
                validBet: Arg.Is(command.Record.ValidBet),
                settle: Arg.Is(command.Record.Settle),
                commission: Arg.Is(command.Record.Commission),
                turnover: Arg.Is(command.Record.Turnover),
                playerWinLoss: Arg.Is(command.Record.WinLoss),
                additionalData: Arg.Is(command.Record.AdditionalData),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);
    }

    [Fact]
    public async void Normal_Process_Settlement_Win()
    {
        var fakeClient = Substitute.For<IGameRoundsWorkClient>();

        var command = new HandleRecordCommand(
            Record: new RecordInfo(
                Success: true,
                GameProviderId: Guid.NewGuid(),
                GameProviderCode: "PPTF",
                ExternalRoundId: "643583c37c18c573b57bbb2c",
                GameId: 1,
                ExternalGameId: "22020",
                PlayerCode: "4uispltzj9d8",
                WalletId: Guid.NewGuid(),
                CurrencyCode: "PHP",
                BrandId: Guid.NewGuid(),
                GameProviderConfigurationId: Guid.NewGuid(),
                Closed: true,
                ExternalCreatedOn: DateTimeOffset.Parse("2023-04-10 23:45:22+08:00"),
                ExternalClosedOn: DateTimeOffset.Parse("2023-04-10 23:45:52+08:00"),
                Bet: 100,
                ValidBet: 100,
                Settle: 200,
                Commission: 0,
                Turnover: 100,
                WinLoss: 100,
                AdditionalData: "{\"recordID\": \"643583c37c18c573b57bbb2c\"}"
                ));

        var service = new HandleRecordCommandService(fakeClient);

        await service
            .ExecuteAsync(
                command)
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .RoundBidding(
                gameProviderId: Arg.Is(command.Record.GameProviderId),
                gameProviderCode: Arg.Is(command.Record.GameProviderCode),
                externalRoundId: Arg.Is(command.Record.ExternalRoundId),
                gameId: Arg.Is(command.Record.GameId),
                externalGameId: Arg.Is(command.Record.ExternalGameId),
                playerCode: Arg.Is(command.Record.PlayerCode),
                walletId: Arg.Is(command.Record.WalletId),
                currencyCode: Arg.Is(command.Record.CurrencyCode),
                brandId: Arg.Is(command.Record.BrandId),
                gameProviderConfigurationId: Arg.Is(command.Record.GameProviderConfigurationId),
                status: Arg.Is(RoundStatus.Closed),
                clientStatus: Arg.Is(RoundClientStatus.Win),
                externalCreatedOn: Arg.Is(command.Record.ExternalCreatedOn),
                externalClosedOn: Arg.Is(command.Record.ExternalClosedOn),
                bet: Arg.Is(command.Record.Bet),
                validBet: Arg.Is(command.Record.ValidBet),
                settle: Arg.Is(command.Record.Settle),
                commission: Arg.Is(command.Record.Commission),
                turnover: Arg.Is(command.Record.Turnover),
                playerWinLoss: Arg.Is(command.Record.WinLoss),
                additionalData: Arg.Is(command.Record.AdditionalData),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);
    }

    [Fact]
    public async void Normal_Process_Success_Is_False()
    {
        var fakeClient = Substitute.For<IGameRoundsWorkClient>();

        var command = new HandleRecordCommand(
            Record: new RecordInfo(
                Success: false,
                GameProviderId: Guid.NewGuid(),
                GameProviderCode: "PPTF",
                ExternalRoundId: "643583c37c18c573b57bbb2c",
                GameId: 1,
                ExternalGameId: "22020",
                PlayerCode: "4uispltzj9d8",
                WalletId: Guid.NewGuid(),
                CurrencyCode: "PHP",
                BrandId: Guid.NewGuid(),
                GameProviderConfigurationId: Guid.NewGuid(),
                Closed: false,
                ExternalCreatedOn: DateTimeOffset.Parse("2023-04-10 23:45:22+08:00"),
                ExternalClosedOn: null,
                Bet: 100,
                ValidBet: 100,
                Settle: 0,
                Commission: 0,
                Turnover: 0,
                WinLoss: 0,
                AdditionalData: "{\"recordID\": \"643583c37c18c573b57bbb2c\"}"
                ));

        var service = new HandleRecordCommandService(fakeClient);

        await service
            .ExecuteAsync(
                command)
            .ConfigureAwait(false);

        await fakeClient
            .Received(0)
            .RoundBidding(
                gameProviderId: Arg.Is(command.Record.GameProviderId),
                gameProviderCode: Arg.Is(command.Record.GameProviderCode),
                externalRoundId: Arg.Is(command.Record.ExternalRoundId),
                gameId: Arg.Is(command.Record.GameId),
                externalGameId: Arg.Is(command.Record.ExternalGameId),
                playerCode: Arg.Is(command.Record.PlayerCode),
                walletId: Arg.Is(command.Record.WalletId),
                currencyCode: Arg.Is(command.Record.CurrencyCode),
                brandId: Arg.Is(command.Record.BrandId),
                gameProviderConfigurationId: Arg.Is(command.Record.GameProviderConfigurationId),
                status: Arg.Is(RoundStatus.Open),
                clientStatus: Arg.Is(RoundClientStatus.Open),
                externalCreatedOn: Arg.Is(command.Record.ExternalCreatedOn),
                externalClosedOn: Arg.Is(command.Record.ExternalClosedOn),
                bet: Arg.Is(command.Record.Bet),
                validBet: Arg.Is(command.Record.ValidBet),
                settle: Arg.Is(command.Record.Settle),
                commission: Arg.Is(command.Record.Commission),
                turnover: Arg.Is(command.Record.Turnover),
                playerWinLoss: Arg.Is(command.Record.WinLoss),
                additionalData: Arg.Is(command.Record.AdditionalData),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .RoundMissing(
                gameProviderId: Arg.Is(command.Record.GameProviderId),
                gameProviderCode: Arg.Is(command.Record.GameProviderCode),
                externalRoundId: Arg.Is(command.Record.ExternalRoundId),
                gameId: Arg.Is(command.Record.GameId),
                externalGameId: Arg.Is(command.Record.ExternalGameId),
                playerCode: Arg.Is(command.Record.PlayerCode),
                data: Arg.Is(JsonSerializer.Serialize(command.Record)),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);
    }

    [Fact]
    public async void Normal_Process_Unsettlement()
    {
        var fakeClient = Substitute.For<IGameRoundsWorkClient>();

        var command = new HandleRecordCommand(
            Record: new RecordInfo(
                Success: true,
                GameProviderId: Guid.NewGuid(),
                GameProviderCode: "PPTF",
                ExternalRoundId: "643583c37c18c573b57bbb2c",
                GameId: 1,
                ExternalGameId: "22020",
                PlayerCode: "4uispltzj9d8",
                WalletId: Guid.NewGuid(),
                CurrencyCode: "PHP",
                BrandId: Guid.NewGuid(),
                GameProviderConfigurationId: Guid.NewGuid(),
                Closed: false,
                ExternalCreatedOn: DateTimeOffset.Parse("2023-04-10 23:45:22+08:00"),
                ExternalClosedOn: null,
                Bet: 100,
                ValidBet: 100,
                Settle: 0,
                Commission: 0,
                Turnover: 0,
                WinLoss: 0,
                AdditionalData: "{\"recordID\": \"643583c37c18c573b57bbb2c\"}"
                ));

        var service = new HandleRecordCommandService(fakeClient);

        await service
            .ExecuteAsync(
                command)
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .RoundBidding(
                gameProviderId: Arg.Is(command.Record.GameProviderId),
                gameProviderCode: Arg.Is(command.Record.GameProviderCode),
                externalRoundId: Arg.Is(command.Record.ExternalRoundId),
                gameId: Arg.Is(command.Record.GameId),
                externalGameId: Arg.Is(command.Record.ExternalGameId),
                playerCode: Arg.Is(command.Record.PlayerCode),
                walletId: Arg.Is(command.Record.WalletId),
                currencyCode: Arg.Is(command.Record.CurrencyCode),
                brandId: Arg.Is(command.Record.BrandId),
                gameProviderConfigurationId: Arg.Is(command.Record.GameProviderConfigurationId),
                status: Arg.Is(RoundStatus.Open),
                clientStatus: Arg.Is(RoundClientStatus.Open),
                externalCreatedOn: Arg.Is(command.Record.ExternalCreatedOn),
                externalClosedOn: Arg.Is(command.Record.ExternalClosedOn),
                bet: Arg.Is(command.Record.Bet),
                validBet: Arg.Is(command.Record.ValidBet),
                settle: Arg.Is(command.Record.Settle),
                commission: Arg.Is(command.Record.Commission),
                turnover: Arg.Is(command.Record.Turnover),
                playerWinLoss: Arg.Is(command.Record.WinLoss),
                additionalData: Arg.Is(command.Record.AdditionalData),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);
    }
}
