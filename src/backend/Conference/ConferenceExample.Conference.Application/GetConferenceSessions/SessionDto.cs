namespace ConferenceExample.Conference.Application.GetConferenceSessions;

public record SessionDto(
    Guid Id,
    string Status,
    DateTimeOffset? SlotStart,
    DateTimeOffset? SlotEnd,
    Guid? RoomId,
    string? RoomName
);
