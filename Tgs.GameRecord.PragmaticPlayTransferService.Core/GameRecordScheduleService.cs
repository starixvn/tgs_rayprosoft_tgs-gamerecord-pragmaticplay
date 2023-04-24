using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.GameRecordSchedules.Client;
using Tgs.GameRecordSchedules.Protos;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

internal class GameRecordScheduleService : IGameRecordScheduleService
{
    private readonly IGameRecordSchedulesClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly double _intervalMinute;
    private readonly ISystemClock _systemClock;

    public GameRecordScheduleService(
        IGameRecordSchedulesClient client,
        ISystemClock systemClock,
        IOptions<GameRecordInfoOptions> options)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        _gameRecordInfo = options.Value;
        _intervalMinute = (_gameRecordInfo.IntervalMinutes > _gameRecordInfo.IntervalMinutesMax)
            ? _gameRecordInfo.IntervalMinutesMax :
            _gameRecordInfo.IntervalMinutes;
    }

    public ValueTask<GameRecordScheduleReply?> GetAsync(
        string dataType,
        CancellationToken cancellationToken = default)
    {
        var request = new GameRecordScheduleGetRequest
        {
            GameProviderCode = _gameRecordInfo.GameProviderCode,
            BrandCode = string.Empty,
            GameProviderAgentId = dataType,
            CurrencyCode = string.Empty
        };

        return _client.GameRecordScheduleGetAsync(request, cancellationToken);
    }

    public ScheduleConfig GetConfig(GameRecordScheduleReply gameRecordSchedule)
        => JsonSerializer.Deserialize<ScheduleConfig>(gameRecordSchedule.Config)!;

    public async ValueTask<GameRecordScheduleReply?> InitAsync(
        Guid gameProviderId,
        string dataType,
        CancellationToken cancellationToken = default)
    {
        var endDate = DateTimeOffset.ParseExact(_systemClock.CurrentTime.ToString(@"yyyy-MM-dd\THH:mm:00K"), @"yyyy-MM-dd\THH:mm:ssK", null, styles: System.Globalization.DateTimeStyles.RoundtripKind);
        var nextTime = endDate.AddMinutes(_intervalMinute * -1);

        var config = new ScheduleConfig
        {
            NextTime = nextTime
        };

        var addRequest = new GameRecordScheduleAddRequest
        {
            GameProviderId = gameProviderId,
            GameProviderCode = _gameRecordInfo.GameProviderCode,
            BrandCode = string.Empty,
            GameProviderAgentId = dataType,
            CurrencyCode = string.Empty,
            Config = JsonSerializer.Serialize(config)
        };

        _ = await _client.GameRecordScheduleAddAsync(addRequest, cancellationToken).ConfigureAwait(false);

        return await GetAsync(dataType, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<GameRecordScheduleReply?> UpdateAsync(
        GameRecordScheduleReply gameRecordSchedule,
        string dataType,
        DateTimeOffset nextTime,
        CancellationToken cancellationToken = default)
    {
        var config = GetConfig(gameRecordSchedule);

        config.NextTime = nextTime;

        gameRecordSchedule.Config = JsonSerializer.Serialize(config);

        var request = new GameRecordScheduleUpdateRequest
        {
            GameRecordScheduleId = gameRecordSchedule.GameRecordScheduleId,
            Config = JsonSerializer.Serialize(config)
        };

        var updateResult = await _client
            .GameRecordScheduleUpdateAsync(request, cancellationToken).ConfigureAwait(false);

        return updateResult.Success
            ? await GetAsync(dataType, cancellationToken).ConfigureAwait(false)
            : null;
    }
}
