using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Diagnostics;
using realworld_net.Dtos;

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
        var errorResponse = new ErrorDto(new Dictionary<string, List<string>>()
        {
            ["username/email"] = new() { "has already been taken" }
        });
        return new ValueTask<bool>(httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken).ContinueWith(_ => true, cancellationToken));

    }
}
