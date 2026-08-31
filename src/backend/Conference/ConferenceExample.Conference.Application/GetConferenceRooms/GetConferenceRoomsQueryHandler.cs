using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.GetConferenceRooms;

public class GetConferenceRoomsQueryHandler(
    IConferenceRepository conferenceRepository,
    ICurrentUserService currentUserService
) : IGetConferenceRoomsQueryHandler
{
    public async Task<IReadOnlyList<GetConferenceRoomsDto>> Handle(GetConferenceRoomsQuery query)
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(query.ConferenceId))
        );

        var currentUserId = currentUserService.GetCurrentUserId();
        var currentOrganizerId = new OrganizerId(new GuidV7(currentUserId));

        if (conference.OrganizerId.Value != currentOrganizerId.Value)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId} is not authorized to view the rooms for conference {conference.Id.Value}. Only the organizer who created the conference can view the rooms."
            );
        }

        return conference
            .Rooms.Select(r => new GetConferenceRoomsDto(r.Id.Value, r.Name.Value))
            .ToList();
    }
}
