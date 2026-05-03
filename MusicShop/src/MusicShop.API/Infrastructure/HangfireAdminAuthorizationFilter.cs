using System.Net;
using Hangfire.Dashboard;

namespace MusicShop.API.Infrastructure;

public sealed class HangfireAdminAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        HttpContext httpContext = context.GetHttpContext();

        // Allow access if running in Development
        string? env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        bool isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

        if (env == "Development")
        {
            // In Docker, IsLocalRequest often fails due to bridge networking.
            // We allow access in Dev mode if it's a local request OR if we are in a container.
            if (isDocker || IsLocalRequest(httpContext))
            {
                return true;
            }
        }

        // Production check: must be authenticated as Admin
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole("Admin");
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
