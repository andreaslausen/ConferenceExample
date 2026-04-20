using ConferenceExample.Authentication;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.GetConferenceSchedule;

public class GetConferenceScheduleQueryHandler(
    IConferenceRepository conferenceRepository,
    ICurrentUserService currentUserService
) : IGetConferenceScheduleQueryHandler
{
    public async Task<IReadOnlyList<GetConferenceScheduleDto>> Handle(
        GetConferenceScheduleQuery query
    )
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(query.ConferenceId))
        );

        var currentUserId = currentUserService.GetCurrentUserId();
        var currentOrganizerId = new OrganizerId(new GuidV7(currentUserId.Value.Value));

        if (conference.OrganizerId.Value != currentOrganizerId.Value)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId.Value} is not authorized to view the schedule for conference {conference.Id.Value}. Only the organizer who created the conference can view the schedule."
            );
        }

        return conference
            .Talks.Select(talk => new GetConferenceScheduleDto(
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
