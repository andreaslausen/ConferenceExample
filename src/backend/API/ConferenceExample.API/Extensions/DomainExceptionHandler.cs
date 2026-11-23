using ConferenceExample.Conference.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SpeakerDomainException = ConferenceExample.Speaker.Domain.DomainException;
using TalkDomainException = ConferenceExample.Talk.Domain.DomainException;

namespace ConferenceExample.API.Extensions;

/// <summary>
/// Maps any bounded context's <c>DomainException</c> (a business rule was violated given the
/// current state of the aggregate) to a 409 Conflict response.
/// </summary>
public class DomainExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is not (DomainException or SpeakerDomainException or TalkDomainException))
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
