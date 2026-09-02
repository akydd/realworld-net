using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace realworld_net.Middleware;

public class DuplicateExceptionHandler : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not UniqueConstraintException uniqueException)
        {
            return new ValueTask<bool>(false);
        }

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        httpContext.Response.ContentType = "application/json";
        var errorResponse = new { Error = "Duplicate entry detected." };
        return new ValueTask<bool>(httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken).ContinueWith(_ => true, cancellationToken));

    }
}