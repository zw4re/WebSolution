using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Worker.Filters
{
    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            if (!httpContext.User.Identity.IsAuthenticated)
            {
                // Yanıtı temizle, yönlendir ve işleme devam etme
                httpContext.Response.StatusCode = 302;
                httpContext.Response.Headers["Location"] = "/login";
                return false;
            }

            return true;
        }
    }
}
