namespace ConferenceExample.Conference.Application.GetConferenceProgram;

public record GetConferenceProgramDto(
    Guid TalkId,
    string TalkTitle,
    string SpeakerName,
    DateTimeOffset? SlotStart,
    DateTimeOffset? SlotEnd,
    Guid? RoomId,
    string? RoomName
);
