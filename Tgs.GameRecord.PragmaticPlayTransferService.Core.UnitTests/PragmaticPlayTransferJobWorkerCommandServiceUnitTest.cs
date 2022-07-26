using System;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Tgs.GameProviders.Client;
using Tgs.GameProviders.Domains;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.ApplicationServices;
using Tgs.GameRecord.PragmaticPlayTransferService.Core.JobWorkerServices;
using Tgs.GameRecordSchedules.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core.UnitTests;

internal class PragmaticPlayTransferJobWorkerCommandServiceUnitTest
{
    [Test]
    public async Task Normal_Process()
    {
        var fakeGameRecordInfoOptions = Options.Create(new GameRecordInfoOptions()
        {
            GameProviderCode = "PPTF",
            GameProviderConfiguration = "PPTFConfiguration",
            IntervalMinute = 10,
            IntervalMinuteMax = 10
        });
        var fakeGameProvidersQueryService = Substitute.For<IUtensil>();
        var fakeGameRecordQueryService = Substitute.For<IGetService<GameRecordQuery, GameRecordResult>>();
        var fakeSystemClock = Substitute.For<ISystemClock>();
        var fakeScheduleDispatch = Substitute.For<IScheduleDispatch>();

        var currentTime = DateTimeOffset.ParseExact(DateTime.UtcNow.ToString(@"yyyy-MM-dd\THH:mm:00K"), @"yyyy-MM-dd\THH:mm:ssK", null, styles: System.Globalization.DateTimeStyles.RoundtripKind);

        var gameProviderId = Guid.Parse("e4ecfa98-b373-4123-87e0-cb9e883d0691");

        
        var dataType = PragmaticPlayTransferGameDataType.RNG;

        var gameRecordResult = new GameRecordResult(
            true,
            10,
            10,
            0,
            currentTime.AddMinutes(8),
            currentTime.AddMinutes(10));

        var gameRecordQuery = new GameRecordQuery(
            dataType,
            currentTime.AddMinutes((double)fakeGameRecordInfoOptions.Value.IntervalMinute * -1));

        var command = new JobWorkerCommand("RNG");

        _ = fakeSystemClock.UtcNow.Returns(currentTime);

        _ = fakeGameProvidersQueryService.GetGameProviderIdAsync()
            .Returns(gameProviderId);

        _ = fakeScheduleDispatch.ExistAsync(Arg.Any<string>())
            .Returns(false);

        _ = fakeScheduleDispatch.LockingAsync(Arg.Any<string>(), command)
            .Returns(true);

        _ = fakeGameRecordQueryService.GetAsync(Arg.Is(gameRecordQuery))
            .Returns(gameRecordResult);

        var sut = new JobWorkerCommandService(
            fakeGameRecordInfoOptions,
            fakeGameProvidersQueryService,
            fakeGameRecordQueryService,
            fakeSystemClock,
            NullLogger<JobWorkerCommandService>.Instance,
            fakeScheduleDispatch);

        await sut.ExecuteAsync(new JobWorkerCommand("RNG")).ConfigureAwait(false);

        _ = fakeGameRecordQueryService
            .Received(1)
            .GetAsync(Arg.Is(gameRecordQuery));

        _ = fakeScheduleDispatch
            .Received(1)
            .UnlockAsync(Arg.Any<string>());
    }
}
