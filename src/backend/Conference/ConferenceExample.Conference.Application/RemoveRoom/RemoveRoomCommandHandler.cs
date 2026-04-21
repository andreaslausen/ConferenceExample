using ConferenceExample.Authentication;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.RoomManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.RemoveRoom;

public class RemoveRoomCommandHandler(
    IConferenceRepository conferenceRepository,
    ICurrentUserService currentUserService
) : IRemoveRoomCommandHandler
{
    public async Task Handle(RemoveRoomCommand command)
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(command.ConferenceId))
        );

        var currentUserId = currentUserService.GetCurrentUserId();
        var currentOrganizerId = new OrganizerId(new GuidV7(currentUserId.Value.Value));

        if (conference.OrganizerId.Value != currentOrganizerId.Value)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId.Value} is not authorized to remove rooms from conference {conference.Id.Value}."
            );
        }

        conference.RemoveRoom(new RoomId(new GuidV7(command.RoomId)));
        await conferenceRepository.Save(conference);
    }
}
