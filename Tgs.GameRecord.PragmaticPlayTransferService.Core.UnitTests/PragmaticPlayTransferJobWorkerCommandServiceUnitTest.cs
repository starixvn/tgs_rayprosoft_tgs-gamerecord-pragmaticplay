using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Tgs.GameProviders.Client;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.JobWorkerServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.UnitTests;

internal class PragmaticPlayTransferJobWorkerCommandServiceUnitTest
{
    [Test]
    public async Task Normal_Process()
    {
        var gameRecordInfoOptions = Options.Create(new GameRecordInfoOptions()
        {
            GameProviderCode = "PPTF",
            GameProviderConfiguration = "PPTFConfiguration",
            MaxIntervalMinute = 10d,
            IntervalMinute = 10d,
            LockMinute = 8.5d
        });
        var fakeLogger = NullLogger<JobWorkerCommandService>.Instance;
        var fakeGameRecordQueryService = Substitute.For<IGetService<GameRecordQuery, GameRecordResult>>();
        var fakeSystemClock = Substitute.For<ISystemClock>();

        var currentTime = DateTimeOffset.UtcNow;

        var startTime = currentTime.AddMinutes(gameRecordInfoOptions.Value.IntervalMinute * -1);

        var query = new JobWorkerQuery(
            Guid.Parse("e4ecfa98-b373-4123-87e0-cb9e883d0691"),
            gameRecordInfoOptions.Value.GameProviderCode,
            PragmaticPlayTransferGameDataType.RNG,
            StartTime: startTime,
            EndTime: currentTime);

        _ = fakeSystemClock.UtcNow.Returns(currentTime);

        var gameRecordResult = new GameRecordResult(
            true,
            10,
            10,
            0,
            startTime.AddMinutes(8),
            startTime.AddMinutes(10));

        var gameRecordQuery = new GameRecordQuery(
            query.GameDataType,
            query.StartTime);

        _ = fakeGameRecordQueryService.GetAsync(Arg.Is(gameRecordQuery))
            .Returns(gameRecordResult);

        var sut = new JobWorkerCommandService(
            fakeLogger,
            fakeGameRecordQueryService,
            fakeSystemClock);

        var actual = await sut.GetAsync(query).ConfigureAwait(false);

        Assert.IsNotNull(actual);
        Assert.IsNotNull(actual.LastGameRecordTime);

        _ = fakeGameRecordQueryService
            .Received(1)
            .GetAsync(Arg.Is(gameRecordQuery));
    }
}
