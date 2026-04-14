using ConferenceExample.Authentication;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.RoomManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;

namespace ConferenceExample.Conference.Application.AssignTalkToRoom;

public class AssignTalkToRoomCommandHandler(
    IConferenceRepository conferenceRepository,
    ICurrentUserService currentUserService
) : IAssignTalkToRoomCommandHandler
{
    public async Task Handle(AssignTalkToRoomCommand command)
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(command.ConferenceId))
        );

        // Check ownership: Only the organizer who created the conference can assign talks to rooms
        var currentUserId = currentUserService.GetCurrentUserId();
        var currentOrganizerId = new OrganizerId(new GuidV7(currentUserId.Value.Value));

        if (conference.OrganizerId.Value != currentOrganizerId.Value)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId.Value} is not authorized to assign talks to rooms for conference {conference.Id.Value}. Only the organizer who created the conference can assign talks to rooms."
            );
        }

        var room = new Room(new RoomId(new GuidV7(command.RoomId)), new Text(command.RoomName));

        conference.AssignTalkToRoom(new TalkId(new GuidV7(command.TalkId)), room);
        await conferenceRepository.Save(conference);
    }
}
