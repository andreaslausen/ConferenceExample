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
/// <remarks>
/// This mapping holds today because every current <c>DomainException</c> describes a conflict
/// with the aggregate's current state. If a future <c>DomainException</c> describes a business
/// rule violation that is not a state conflict, revisit this mapping (and possibly introduce a
/// more specific exception type) rather than defaulting it to 409.
/// </remarks>
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
