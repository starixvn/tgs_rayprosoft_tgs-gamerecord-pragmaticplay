using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Tgs.GameProviders.Domains;
using Tgs.GameProviders.QueryServer.Models;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;
using Tgs.MessageQueue;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.UnitTests;

internal class GameRecordHandleServiceUnitTests
{
    [Test]
    public async Task Normal_Process()
    {
        var fakeOptions = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = GameProviderType.PragmaticPlayTransfer,
            GameProviderConfiguration = GameProviderConfigurations.PragmaticPlayTransfer,
            IntervalMinuteMax = 10,
            IntervalMinute = 10
        });

        var fakeUtensil = Substitute.For<IUtensil>();
        var fakeMessageSender = Substitute.For<IMessageSender>();
        var fakeLogger = NullLogger<GameRecordHandleService>.Instance;

        var gameRecord = new GameRecordInfo(
            GameProviderType.PragmaticPlayTransfer,
            Guid.Parse("e4ecfa98-b373-4123-87e0-cb9e883d0691"),
            "4ucmqplxb029",
            RoundStatus.Closed,
            RoundClientStatus.Win,
            "38660",
            "vpmr9",
            DateTimeOffset.UtcNow.AddSeconds(-5),
            DateTimeOffset.UtcNow,
            10m,
            10m,
            10m,
            null,
            10m,
            20m,
            null,
            string.Empty);

        var playerInfo = new PlayerModel(
            Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
            "TGS",
            Guid.Parse("36a6cdef-094f-eb11-8d88-70b5e82b6b8d"),
            "TGS",
            Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
            "GPI",
            gameRecord.PlayerCode,
            gameRecord.PlayerCode,
            Guid.Parse("36a6cdef-094f-eb11-8d88-70b5e82b6b8d"),
            "CNY",
            PlayerStatus.Enabled);

        var gameProviderConfigurationId = Guid.NewGuid();

        _ = fakeUtensil.GetPlayerAsync(
                Arg.Is(gameRecord.PlayerCode))
            .Returns(playerInfo);

        _ = fakeUtensil.GetGameIdAsync(
                Arg.Is(gameRecord.GameProviderId),
                Arg.Is(gameRecord.ExternalGameId))
            .Returns(2700258);

        _ = fakeUtensil.GetBrandGetGameProviderConfigurationIdAsync(
                Arg.Is(playerInfo.BrandId),
                Arg.Is(gameRecord.GameProviderId))
            .Returns(gameProviderConfigurationId);

        var query = new GameRoundHandleQuery(gameRecord);

        var sut = new GameRecordHandleService(
            fakeUtensil,
            fakeMessageSender,
            fakeOptions,
            fakeLogger);

        var actual = await sut.GetAsync(query);

        Assert.True(actual);

        _ = fakeMessageSender
            .Received(1)
            .PublishAsync(
                Arg.Is("tgs.gp.gamerecord.database.round.pptf"),
                Arg.Any<ReadOnlyMemory<byte>>());
    }
}
