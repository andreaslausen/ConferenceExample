namespace ConferenceExample.Conference.Application.AddRoom;

public record AddRoomCommand(Guid ConferenceId, string Name);
