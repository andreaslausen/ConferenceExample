using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.RoomManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.AddRoom;

public class AddRoomCommandHandler(
    IConferenceRepository conferenceRepository,
    ICurrentUserService currentUserService
) : IAddRoomCommandHandler
{
    public async Task<RoomAddedDto> Handle(AddRoomCommand command)
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(command.ConferenceId))
        );

        var currentUserId = currentUserService.GetCurrentUserId();
        var currentOrganizerId = new OrganizerId(new GuidV7(currentUserId));

        if (conference.OrganizerId.Value != currentOrganizerId.Value)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId} is not authorized to add rooms to conference {conference.Id.Value}."
            );
        }

        var roomId = new RoomId(GuidV7.NewGuid());
        conference.AddRoom(roomId, new Text(command.Name));

        await conferenceRepository.Save(conference);

        return new RoomAddedDto(roomId.Value, command.Name);
    }
}
