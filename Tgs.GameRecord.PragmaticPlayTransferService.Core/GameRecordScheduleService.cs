using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.GameProviders.Client;
using Tgs.GameRecordSchedules.Client;
using Tgs.GameRecordSchedules.Protos;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

internal class GameRecordScheduleService : IGameRecordScheduleService
{
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly IGameRecordSchedulesGrpc _scheduleClient;
    private readonly ISystemClock _systemClock;

    public GameRecordScheduleService(
        IGameRecordSchedulesGrpc scheduleClient,
        ISystemClock systemClock,
        IOptions<GameRecordInfoOptions> options)
    {
        _scheduleClient = scheduleClient ?? throw new ArgumentNullException(nameof(scheduleClient));
        _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        _gameRecordInfo = options.Value;
    }

    public ValueTask<GameRecordScheduleReply?> GetAsync(
        PragmaticPlayTransferGameDataType gameDataType,
        CancellationToken cancellationToken)
    {
        var request = new GameRecordScheduleGetRequest
        {
            GameProviderCode = $"{_gameRecordInfo.GameProviderCode}_{gameDataType}",
            BrandCode = string.Empty,
            GameProviderAgentId = string.Empty,
            CurrencyCode = string.Empty
        };

        return _scheduleClient.GameRecordScheduleGetAsync(request, cancellationToken);
    }

    public ScheduleConfig GetConfig(GameRecordScheduleReply gameProviderSchedule)
        => JsonSerializer.Deserialize<ScheduleConfig>(gameProviderSchedule.Config)!;

    public async ValueTask<GameRecordScheduleReply?> InitAsync(
        Guid gameProviderId,
        PragmaticPlayTransferGameDataType gameDataType,
        CancellationToken cancellationToken)
    {
        var sysTime = _systemClock.CurrentTime;

        var config = new ScheduleConfig
        {
            LastGameRecordTime = new DateTimeOffset(Convert.ToDateTime("1970-01-01"), TimeSpan.Zero),
            StartTime = sysTime.AddSeconds((double)_gameRecordInfo.IntervalMinute * 60 * -1),
            EndTime = sysTime,
            NextTime = sysTime,
            DataType = gameDataType
        };

        var addRequest = new GameRecordScheduleAddRequest
        {
            GameProviderId = gameProviderId,
            GameProviderCode = $"{_gameRecordInfo.GameProviderCode}_{gameDataType}",
            BrandCode = string.Empty,
            GameProviderAgentId = string.Empty,
            CurrencyCode = string.Empty,
            Config = JsonSerializer.Serialize(config)
        };

        _ = await _scheduleClient.GameRecordScheduleAddAsync(addRequest, cancellationToken).ConfigureAwait(false);

        return await GetAsync(gameDataType, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<GameRecordScheduleReply?> UpdateAsync(
        GameRecordScheduleReply gameProviderSchedule,
        DateTimeOffset? lastGameRecordTime,
        DateTimeOffset nextTime,
        DateTimeOffset sysTime,
        CancellationToken cancellationToken)
    {
        var intervalMinute = _gameRecordInfo.IntervalMinute;
        var config = GetConfig(gameProviderSchedule);

        if (config.EndTime > sysTime || (sysTime - config.EndTime).TotalSeconds < (double)intervalMinute * 60)
        {
            config.StartTime = sysTime.AddSeconds((double)intervalMinute * 60 * -1);
            config.EndTime = sysTime;
        }
        else
        {
            var endTime = config.EndTime.AddSeconds((double)intervalMinute * 60);
            if (endTime > nextTime)
                endTime = nextTime;

            config.StartTime = config.EndTime;
            config.EndTime = endTime > sysTime ? sysTime : endTime;
        }

        config.LastGameRecordTime = lastGameRecordTime ?? config.LastGameRecordTime;
        config.NextTime = nextTime;

        gameProviderSchedule.Config = JsonSerializer.Serialize(config);

        var request = new GameRecordScheduleUpdateRequest
        {
            GameRecordScheduleId = gameProviderSchedule.GameRecordScheduleId,
            Config = JsonSerializer.Serialize(config)
        };

        var updateResult = await _scheduleClient
            .GameRecordScheduleUpdateAsync(request, cancellationToken).ConfigureAwait(false);

        return updateResult.Success
            ? await GetAsync(config.DataType, cancellationToken).ConfigureAwait(false)
            : null;
    }
}
