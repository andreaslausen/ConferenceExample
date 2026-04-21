using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;

namespace ConferenceExample.Conference.Application.GetConferenceSchedule;

public class GetConferenceScheduleQueryHandler(
    IConferenceRepository conferenceRepository,
    IConferenceTalkReadModelRepository talkReadModelRepository,
    ICurrentUserService currentUserService
) : IGetConferenceScheduleQueryHandler
{
    public async Task<IReadOnlyList<GetConferenceScheduleDto>> Handle(
        GetConferenceScheduleQuery query
    )
    {
        var conferenceId = new ConferenceId(new GuidV7(query.ConferenceId));
        var conference = await conferenceRepository.GetById(conferenceId);

        var currentUserId = currentUserService.GetCurrentUserId();
        var currentOrganizerId = new OrganizerId(new GuidV7(currentUserId));

        if (conference.OrganizerId.Value != currentOrganizerId.Value)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId} is not authorized to view the schedule for conference {conference.Id.Value}. Only the organizer who created the conference can view the schedule."
            );
        }

        var talks = await talkReadModelRepository.GetByConferenceId(conferenceId);

        return talks
            .Select(talk => new GetConferenceScheduleDto(
                talk.Id,
                talk.Title,
                talk.Status,
                talk.SlotStart,
                talk.SlotEnd,
                talk.RoomId,
                talk.RoomName
            ))
            .ToList();
    }
}
