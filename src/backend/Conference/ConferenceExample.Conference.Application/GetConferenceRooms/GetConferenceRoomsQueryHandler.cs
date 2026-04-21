using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.GetConferenceRooms;

public class GetConferenceRoomsQueryHandler(IConferenceRepository conferenceRepository)
    : IGetConferenceRoomsQueryHandler
{
    public async Task<IReadOnlyList<GetConferenceRoomsDto>> Handle(GetConferenceRoomsQuery query)
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(query.ConferenceId))
        );

        return conference
            .Rooms.Select(r => new GetConferenceRoomsDto(r.Id.Value, r.Name.Value))
            .ToList();
    }
}
