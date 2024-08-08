using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Tgs.GameRecord.PragmaticPlay.Core;
using Tgs.GameRecord.PragmaticPlay.Core.Models;
using Tgs.GameRecord.PragmaticPlay.Repositories.Extensions;
using Tgs.GameRecord.PragmaticPlay.Repositories.Models;

namespace Tgs.GameRecord.PragmaticPlay.Repositories;

internal class GameRecordScheduleRepository(
	ILogger<GameRecordScheduleRepository> logger,
	IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
	IMongoClient client) : IGameRecordScheduleRepository
{
	private readonly GameRecordInfoOptions _gameRecordInfoOptions = gameRecordInfoOptions.Value;

	public async ValueTask<GameRecordScheduleInfo> AddAsync(string secureLogin, string dataType, DateTimeOffset nextTime, CancellationToken cancellationToken = default)
	{
		var collection = GetCollection();
		var document = new GameRecordSchedule()
		{
			SecureLogin = secureLogin,
			DataType = dataType,
			NextTime = nextTime.UtcDateTime,
			CreatedAt = DateTimeOffset.UtcNow.UtcDateTime,
			UpdatedAt = DateTimeOffset.UtcNow.UtcDateTime
		};

		await collection.InsertOneAsync(
			document: document,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		return new GameRecordScheduleInfo(
			SecureLogin: document.SecureLogin,
			DataType: document.DataType,
			NextTime: document.NextTime,
			CreatedAt: document.CreatedAt,
			UpdatedAt: document.UpdatedAt);
	}

	public async ValueTask<GameRecordScheduleInfo?> GetAsync(string secureLogin, string dataType, CancellationToken cancellationToken = default)
	{
		var collection = GetCollection();
		var builder = Builders<GameRecordSchedule>.Filter;
		var filter = builder.Eq(x => x.SecureLogin, secureLogin)
			& builder.Eq(x => x.DataType, dataType);

		var gameRecordSchedule = (await collection.FindAsync<GameRecordSchedule>(
			filter: filter,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false))
			.FirstOrDefault(cancellationToken: cancellationToken);

		return gameRecordSchedule is null
			? null
			: new GameRecordScheduleInfo(
				SecureLogin: gameRecordSchedule.SecureLogin,
				DataType: gameRecordSchedule.DataType,
				NextTime: gameRecordSchedule.NextTime,
				CreatedAt: gameRecordSchedule.CreatedAt,
				UpdatedAt: gameRecordSchedule.UpdatedAt);
	}

	public async ValueTask InitializationAsync(CancellationToken cancellationToken = default)
	{
		var collection = GetCollection();

		var tasks = new Task[]{
			Task.Run(async () =>
			{
				try
				{
					_ = await Index_UniqueAsync( collection: collection,   cancellationToken: cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "LogOn:{logOn} Message:This index creation failed.{index}",  DateTime.Now.ToString("u"),"Index_Unique");
				}
			}, cancellationToken: cancellationToken),
			Task.Run(async () =>
			{
				try
				{
					 _ = await Index_UpdatedAtAsync( collection: collection,   cancellationToken: cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "LogOn:{logOn} Message:This index creation failed.{index}",  DateTime.Now.ToString("u"),"Index_ExpireOn");
				}
			}, cancellationToken: cancellationToken)
		};

		await Task.WhenAll(tasks).ConfigureAwait(false);
	}

	public async ValueTask UpdateAsync(string secureLogin, string dataType, DateTimeOffset nextTime, CancellationToken cancellationToken = default)
	{
		var collection = GetCollection();
		var builder = Builders<GameRecordSchedule>.Filter;
		var filter = builder.Eq(x => x.SecureLogin, secureLogin)
			& builder.Eq(x => x.DataType, dataType);

		var update = Builders<GameRecordSchedule>.Update
			.Set(s => s.NextTime, nextTime.UtcDateTime)
			.Set(s => s.UpdatedAt, DateTimeOffset.UtcNow.UtcDateTime);

		_ = await collection.UpdateOneAsync(
			filter: filter,
			update: update,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	private static Task<string> Index_UpdatedAtAsync(IMongoCollection<GameRecordSchedule> collection, CancellationToken cancellationToken = default)
	{
		var index = Builders<GameRecordSchedule>.IndexKeys
			.Ascending(m => m.UpdatedAt);

		var createIndexOptions = new CreateIndexOptions
		{
			ExpireAfter = TimeSpan.FromDays(7),
			Name = "idx_updated_at"
		};
		var createIndexModel = new CreateIndexModel<GameRecordSchedule>(index, createIndexOptions);

		return collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken);
	}

	private static Task<string> Index_UniqueAsync(IMongoCollection<GameRecordSchedule> collection, CancellationToken cancellationToken = default)
	{
		var index = Builders<GameRecordSchedule>.IndexKeys
			.Ascending(m => m.SecureLogin)
			.Ascending(m => m.DataType);

		var createIndexOptions = new CreateIndexOptions
		{
			Unique = true,
			Name = $"uni_{collection.CollectionNamespace.CollectionName}"
		};
		var createIndexModel = new CreateIndexModel<GameRecordSchedule>(index, createIndexOptions);

		return collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken);
	}

	private IMongoCollection<GameRecordSchedule> GetCollection()
		=> client
		.GetDatabase($"game_provider_{_gameRecordInfoOptions.GameProviderCode}_master".ToLower())
		.GetCollection<GameRecordSchedule>(typeof(GameRecordSchedule).GetTableName());
}
