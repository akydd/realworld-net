using Microsoft.AspNetCore.Diagnostics;
using realworld_net.Dtos;
using realworld_net.Exceptions;

namespace realworld_net.Middleware;

public class AppExceptionHandler : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not AppException)
        {
            return new ValueTask<bool>(false);
        }

        int status = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ForbiddenException => StatusCodes.Status403Forbidden,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        AppException ex = (AppException)exception;

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/json";
        var errorResponse = new ErrorDto(new Dictionary<string, List<string>>
        {
            [ex.Field] = new List<string> { ex.Error }
        });
        return new ValueTask<bool>(httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken).ContinueWith(_ => true, cancellationToken));
    }
}
