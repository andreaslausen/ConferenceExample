using ConferenceExample.Authentication;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.GetConferenceSessions;

public class GetConferenceSessionsQueryHandler(
    IConferenceRepository conferenceRepository,
    ICurrentUserService currentUserService
) : IGetConferenceSessionsQueryHandler
{
    public async Task<IReadOnlyList<GetConferenceSessionDto>> Handle(
        GetConferenceSessionsQuery query
    )
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(query.ConferenceId))
        );

        // Check ownership: Only the organizer who created the conference can view sessions
        var currentUserId = currentUserService.GetCurrentUserId();
        var currentOrganizerId = new OrganizerId(new GuidV7(currentUserId.Value.Value));

        if (conference.OrganizerId.Value != currentOrganizerId.Value)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId.Value} is not authorized to view sessions for conference {conference.Id.Value}. Only the organizer who created the conference can view sessions."
            );
        }

        return conference
            .Talks.Select(talk => new GetConferenceSessionDto(
                talk.Id.Value,
                talk.Status.ToString(),
                talk.Slot?.Start,
                talk.Slot?.End,
                talk.Room?.Id.Value.Value,
                talk.Room?.Name.Value
            ))
            .ToList();
    }
}
