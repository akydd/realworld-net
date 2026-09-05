using Microsoft.AspNetCore.Diagnostics;

namespace realworld_net.Middleware;

public class UnauthorizedAccessExceptionHandler : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not UnauthorizedAccessException)
        {
            return new ValueTask<bool>(false);
        }

        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        httpContext.Response.ContentType = "application/json";
        var errorResponse = new { Error = "Unauthorized access." };
        return new ValueTask<bool>(httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken).ContinueWith(_ => true, cancellationToken));
    }
}
