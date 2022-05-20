using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NUnit.Framework;
using Tgs.GameProviders.Client;
using Tgs.GameProviders.Domains;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;
using Tgs.GameRecordSchedules.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.UnitTests;

internal class PragmaticPlayTransferJobWorkerCommandServiceUnitTest
{
    [Test]
    public async Task Normal_Process()
    {
        var fakeGameRecordScheduleService = Substitute.For<IGameRecordScheduleService>();
        var fakeGameProvidersQueryService = Substitute.For<IGameProvidersQueryService>();
        var fakeGameRecordQueryService = Substitute.For<IGetService<GameRecordQuery, GameRecordResult>>();

        var gameProviderId = Guid.Parse("e4ecfa98-b373-4123-87e0-cb9e883d0691");

        var scheduleConfig = new ScheduleConfig
        {
            IntervalMinuteMax = 60m,
            IntervalMinute = 20m,
            LastGameRecordTime = new DateTimeOffset(Convert.ToDateTime("1970-01-01"), TimeSpan.Zero),
            StartTime = DateTimeOffset.UtcNow.AddMinutes(-10),
            EndTime = DateTimeOffset.UtcNow,
            NextTime = DateTimeOffset.UtcNow
        };

        var dataType = PragmaticPlayTransferGameDataType.RNG;

        var schedule = new GameRecordScheduleReply
        {
            BrandCode = string.Empty,
            GameProviderAgentId = string.Empty,
            CurrencyCode = string.Empty,
            GameProviderId = gameProviderId,
            GameProviderCode = $"{GameProviderType.PragmaticPlayTransfer}_{dataType}",
            Config = JsonSerializer.Serialize(scheduleConfig)
        };

        var gameRecordResult = new GameRecordResult(
            true,
            10,
            10,
            0,
            DateTimeOffset.UtcNow.AddMinutes(8),
            DateTimeOffset.UtcNow.AddMinutes(10));

        var gameRecordQuery = new GameRecordQuery(
            dataType,
            scheduleConfig.StartTime);

        _ = fakeGameProvidersQueryService.GetGameProviderIdAsync(Arg.Is(GameProviderType.PragmaticPlayTransfer))
         .Returns(gameProviderId);

        _ = fakeGameRecordScheduleService
            .GetAsync(
                Arg.Is(dataType))
            .Returns(schedule);

        _ = fakeGameRecordScheduleService.GetConfig(Arg.Is(schedule))
            .Returns(scheduleConfig);

        _ = fakeGameRecordQueryService.GetAsync(Arg.Is(gameRecordQuery))
            .Returns(gameRecordResult);

        var sut = new PragmaticPlayTransferJobWorkerCommandService(
            fakeGameProvidersQueryService,
            fakeGameRecordScheduleService,
            fakeGameRecordQueryService,
            NullLogger<PragmaticPlayTransferJobWorkerCommandService>.Instance);

        await sut.ExecuteAsync(new PragmaticPlayTransferJobWorkerCommand()).ConfigureAwait(false);

        _ = fakeGameRecordScheduleService
            .Received(1)
            .UpdateAsync(
                Arg.Any<GameRecordScheduleReply>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>());

        _ = fakeGameRecordQueryService
            .Received(1)
            .GetAsync(Arg.Is(gameRecordQuery));
    }
}
