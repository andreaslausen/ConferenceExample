using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceExample.API.Extensions;

/// <summary>
/// Maps <see cref="UnauthorizedAccessException"/> thrown by application-layer ownership
/// checks (e.g. "you are not the organizer of this conference") to a 403 Forbidden response,
/// matching the controllers' <c>[ProducesResponseType(StatusCodes.Status403Forbidden)]</c>.
/// </summary>
public class AuthorizationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is not UnauthorizedAccessException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = exception.Message,
            },
            cancellationToken
        );

        return true;
    }
}
