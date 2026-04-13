namespace ConferenceExample.Conference.Application.GetConferenceSessions;

public record GetConferenceSessionDto(
    Guid Id,
    string Status,
    DateTimeOffset? SlotStart,
    DateTimeOffset? SlotEnd,
    Guid? RoomId,
    string? RoomName
);
