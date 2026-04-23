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
        var currentOrganizerId = new OrganizerId(new GuidV7(currentUserId));

        if (conference.OrganizerId.Value != currentOrganizerId.Value)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId} is not authorized to assign talks to rooms for conference {conference.Id.Value}. Only the organizer who created the conference can assign talks to rooms."
            );
        }

        var roomId = new RoomId(new GuidV7(command.RoomId));
        var room =
            conference.Rooms.FirstOrDefault(r => r.Id == roomId)
            ?? throw new InvalidOperationException(
                $"Room '{command.RoomId}' does not exist in conference '{command.ConferenceId}'."
            );

        conference.AssignTalkToRoom(new TalkId(new GuidV7(command.TalkId)), room);
        await conferenceRepository.Save(conference);
    }
}
