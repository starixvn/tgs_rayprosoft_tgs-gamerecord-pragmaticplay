namespace Tgs.GameRecord.PragmaticPlay.Repositories.UnitTest;

public class UtensilRepositoryUnitTest
{
	[Fact]
	public void Is_Rund_Greater_Than_Current_Time()
	{
		var sut = new UtensilRepository();

		var nextTime = DateTime.Parse("2023-04-29T15:15:00+00:00");
		var endTime = DateTime.Parse("2023-04-29T15:01:00+00:00");
		var currentTime = DateTime.Parse("2023-04-29T15:00:30+00:00");
		var actual = sut.IsRun(nextTime, endTime, currentTime);

		Assert.False(actual);
	}

	[Fact]
	public void Is_Rund_Greater_Than_End_Time()
	{
		var sut = new UtensilRepository();

		var nextTime = DateTime.Parse("2023-04-29T15:00:00+00:00");
		var endTime = DateTime.Parse("2023-04-29T15:00:00+00:00");
		var currentTime = DateTime.Parse("2023-04-29T15:00:00+00:00");

		var actual = sut.IsRun(nextTime, endTime, currentTime);

		Assert.False(actual);
	}

	[Fact]
	public void Is_Rund_True()
	{
		var sut = new UtensilRepository();

		var nextTime = DateTime.Parse("2023-04-29T15:00:00+00:00");
		var endTime = DateTime.Parse("2023-04-29T15:00:31+00:00");
		var currentTime = DateTime.Parse("2023-04-29T15:01:00+00:00");
		var actual = sut.IsRun(nextTime, endTime, currentTime);

		Assert.True(actual);
	}
}
