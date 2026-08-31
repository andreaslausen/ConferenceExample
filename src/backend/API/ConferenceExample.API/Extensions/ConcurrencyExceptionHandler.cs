using ConferenceExample.EventStore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceExample.API.Extensions;

/// <summary>
/// Maps <see cref="ConcurrencyException"/> (an optimistic-concurrency conflict on the event
/// store) to a 409 Conflict response.
/// </summary>
public class ConcurrencyExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is not ConcurrencyException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = exception.Message,
            },
            cancellationToken
        );

        return true;
    }
}
