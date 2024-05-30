using Hangfire.Dashboard;

namespace Tgs.GameRecord.PragmaticPlay;

public class IgnoreHangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
	public bool Authorize(DashboardContext context) => true;
}
