using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameCommand.Client;
using Tgs.GameCommand.Common;

namespace Tgs.GameRecord.PragmaticPlay.Core.UnitTest;

public class UtensilRepositoryUnitTest
{
    [Fact]
    public async Task Get_GameProvider_Id()
    {
        var gameRecordInfo = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "PPTF",
            GameProviderConfiguration = "PPTFConfiguration",
            GameProviderHangfire = "pptf-schedule:hangfire:",
            GameDataTypes = new[] { "RNG", "LC", "VSB" },
            IntervalMinutes = 1,
            IntervalMinutesMax = 10,
            IntervalMinutesMinimum = 1,
            VirtualSportsIntervalMinutes = 10,
            LiveDealerIntervalMinutes = 5,
            LockMinute = 8
        });
        var fakeClient = Substitute.For<IGameCommandClient>();
        var fakeMemoryCache = Substitute.For<IMemoryCache>();

        var key = $"gamerecord:{gameRecordInfo.Value.GameProviderCode}";

        _ = fakeMemoryCache
            .TryGetValue(
                key: Arg.Is(key),
                value: out _)
             .Returns(false);

        var gameProviderId = Guid.NewGuid();
        _ = fakeClient
            .GameProviderGetGameProviderIdAsync(
                gameProviderCode: Arg.Is(gameRecordInfo.Value.GameProviderCode),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameProviderId);

        var repository = new UtensilRepository(
            gameRecordInfo,
            fakeClient,
            fakeMemoryCache);

        var actual = await repository
            .GetGameProviderIdAsync()
            .ConfigureAwait(false);

        Assert.Equal(gameProviderId, actual);

        _ = fakeMemoryCache
            .Received(1)
            .Set(
                key: key,
                gameProviderId);
    }


    [Fact]
    public void Is_Rund_Greater_Than_Current_Time()
    {
        var gameRecordInfo = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "PPTF",
            GameProviderConfiguration = "PPTFConfiguration",
            GameProviderHangfire = "pptf-schedule:hangfire:",
            GameDataTypes = new[] { "RNG", "LC", "VSB" },
            IntervalMinutes = 1,
            IntervalMinutesMax = 10,
            IntervalMinutesMinimum = 1,
            VirtualSportsIntervalMinutes = 10,
            LiveDealerIntervalMinutes = 5,
            LockMinute = 8
        });
        var fakeClient = Substitute.For<IGameCommandClient>();
        var fakeMemoryCache = Substitute.For<IMemoryCache>();

        var repository = new UtensilRepository(
            gameRecordInfo,
            fakeClient,
            fakeMemoryCache);

        var nextTime = DateTime.Parse("2023-04-29T15:15:00+00:00");
        var endTime = DateTime.Parse("2023-04-29T15:01:00+00:00");
        var currentTime = DateTime.Parse("2023-04-29T15:00:30+00:00");
        var actual = repository.IsRun(nextTime, endTime, currentTime);

        Assert.False(actual);
    }

    [Fact]
    public void Is_Rund_Greater_Than_End_Time()
    {
        var gameRecordInfo = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "PPTF",
            GameProviderConfiguration = "PPTFConfiguration",
            GameProviderHangfire = "pptf-schedule:hangfire:",
            GameDataTypes = new[] { "RNG", "LC", "VSB" },
            IntervalMinutes = 1,
            IntervalMinutesMax = 10,
            IntervalMinutesMinimum = 1,
            VirtualSportsIntervalMinutes = 10,
            LiveDealerIntervalMinutes = 5,
            LockMinute = 8
        });
        var fakeClient = Substitute.For<IGameCommandClient>();
        var fakeMemoryCache = Substitute.For<IMemoryCache>();

        var repository = new UtensilRepository(
            gameRecordInfo,
            fakeClient,
            fakeMemoryCache);

        var nextTime = DateTime.Parse("2023-04-29T15:00:00+00:00");
        var endTime = DateTime.Parse("2023-04-29T15:00:00+00:00");
        var currentTime = DateTime.Parse("2023-04-29T15:00:00+00:00");

        var actual = repository.IsRun(nextTime, endTime, currentTime);

        Assert.False(actual);
    }

    [Fact]
    public void Is_Rund_True()
    {
        var gameRecordInfo = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "PPTF",
            GameProviderConfiguration = "PPTFConfiguration",
            GameProviderHangfire = "pptf-schedule:hangfire:",
            GameDataTypes = new[] { "RNG", "LC", "VSB" },
            IntervalMinutes = 1,
            IntervalMinutesMax = 10,
            IntervalMinutesMinimum = 1,
            VirtualSportsIntervalMinutes = 10,
            LiveDealerIntervalMinutes = 5,
            LockMinute = 8
        });
        var fakeClient = Substitute.For<IGameCommandClient>();
        var fakeMemoryCache = Substitute.For<IMemoryCache>();

        var repository = new UtensilRepository(
            gameRecordInfo,
            fakeClient,
            fakeMemoryCache);

        var nextTime = DateTime.Parse("2023-04-29T15:00:00+00:00");
        var endTime = DateTime.Parse("2023-04-29T15:00:31+00:00");
        var currentTime = DateTime.Parse("2023-04-29T15:01:00+00:00");
        var actual = repository.IsRun(nextTime, endTime, currentTime);

        Assert.True(actual);
    }
}
