namespace ConferenceExample.Conference.Application.AssignTalkToRoom;

public record AssignTalkToRoomCommand(Guid ConferenceId, Guid TalkId, Guid RoomId, string RoomName);
