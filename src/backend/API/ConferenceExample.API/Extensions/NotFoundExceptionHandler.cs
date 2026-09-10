using ConferenceExample.Conference.Domain.SharedKernel;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SpeakerNotFoundException = ConferenceExample.Speaker.Domain.SharedKernel.NotFoundException;
using TalkNotFoundException = ConferenceExample.Talk.Domain.SharedKernel.NotFoundException;

namespace ConferenceExample.API.Extensions;

/// <summary>
/// Maps any bounded context's <c>NotFoundException</c> (entity looked up by id does not exist)
/// to a 404 Not Found response.
/// </summary>
public class NotFoundExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (
            exception
            is not (NotFoundException or SpeakerNotFoundException or TalkNotFoundException)
        )
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = exception.Message,
            },
            cancellationToken
        );

        return true;
    }
}
