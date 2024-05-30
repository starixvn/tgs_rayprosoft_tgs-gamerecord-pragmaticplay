using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CronExpressionDescriptor;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Tgs.GameRecord.PragmaticPlay.Core;

namespace Tgs.GameRecord.PragmaticPlay.Repositories;
internal class GameRecordScheduleLockRepository(
	IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
	IDatabase database,
	TimeProvider timeProvider) : IGameRecordScheduleLockRepository
{
	private readonly GameRecordInfoOptions _gameRecordInfoOptions = gameRecordInfoOptions.Value;
	public Task<bool> LockingAsync(string secureLogin, string dataType, CancellationToken cancellationToken = default)
		=> database.StringSetAsync(
			key: GetKey(secureLogin, dataType),
			value: timeProvider.GetUtcNow().ToString("O"),
			expiry: TimeSpan.FromMinutes(_gameRecordInfoOptions.LockMinute),
			when: When.NotExists);

	public Task<bool> UnlockAsync(string secureLogin, string dataType, CancellationToken cancellationToken = default)
		=> database.KeyDeleteAsync(
			key: GetKey(secureLogin, dataType),
			flags: CommandFlags.FireAndForget);

	private static string GetKey(string secureLogin, string dataType)
	=> string.Concat(typeof(GameRecordScheduleLockRepository).FullName!.Replace(".", "-").ToLower(), ":", secureLogin, ":", dataType);

}
