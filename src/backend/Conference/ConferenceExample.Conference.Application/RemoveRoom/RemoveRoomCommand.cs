namespace ConferenceExample.Conference.Application.RemoveRoom;

public record RemoveRoomCommand(Guid ConferenceId, Guid RoomId);
