using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Aurora.Presentation.Services;

public class HangfireAuthorization : IDashboardAuthorizationFilter
{
    public bool Authorize([NotNull] DashboardContext context)
    {
        return true;
    }
}