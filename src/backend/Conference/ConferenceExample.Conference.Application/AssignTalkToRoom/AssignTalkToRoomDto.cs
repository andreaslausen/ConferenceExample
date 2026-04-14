namespace ConferenceExample.Conference.Application.AssignTalkToRoom;

public class AssignTalkToRoomDto
{
    public required Guid RoomId { get; init; }
    public required string RoomName { get; init; }
}
