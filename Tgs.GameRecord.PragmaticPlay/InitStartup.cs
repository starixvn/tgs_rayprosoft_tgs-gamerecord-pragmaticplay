using Tgs.GameRecord.PragmaticPlay.Core;

namespace Tgs.GameRecord.PragmaticPlay;

public sealed class InitStartup(IGameRecordScheduleRepository gameRecordScheduleRepository) : IHostedService
{
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await gameRecordScheduleRepository.InitializationAsync(
			cancellationToken)
			.ConfigureAwait(false);
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
