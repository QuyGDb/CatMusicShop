using System.Net;
using Hangfire.Dashboard;

namespace MusicShop.API.Infrastructure;

public sealed class HangfireAdminAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        HttpContext httpContext = context.GetHttpContext();

        // Allow access if running locally in Development
        string? env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (env == "Development" && IsLocalRequest(httpContext))
        {
            return true;
        }

        // Production check: must be authenticated as admin
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole("admin");
    }

    private static bool IsLocalRequest(HttpContext context)
    {
        var connection = context.Connection;
        if (connection.RemoteIpAddress != null)
        {
            if (connection.LocalIpAddress != null)
            {
                return connection.RemoteIpAddress.Equals(connection.LocalIpAddress);
            }
            return IPAddress.IsLoopback(connection.RemoteIpAddress);
        }

        // for HTTP/2, RemoteIpAddress is null for local requests
        return connection.RemoteIpAddress == null && connection.LocalIpAddress == null;
    }
}
