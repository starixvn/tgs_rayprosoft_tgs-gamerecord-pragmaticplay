using Microsoft.Extensions.Hosting;

using Tgs.GameRecord.PragmaticPlay.Core;

namespace Tgs.GameRecord.PragmaticPlay.Repositories;

public sealed class GameRecordScheduleInitStartup(IGameRecordScheduleRepository gameRecordScheduleRepository) : IHostedService
{
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await gameRecordScheduleRepository.InitializationAsync(
			cancellationToken)
			.ConfigureAwait(false);
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
