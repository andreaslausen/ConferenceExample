namespace ConferenceExample.Conference.Application.Dtos;

public record SessionDto(
    Guid Id,
    string Status,
    DateTimeOffset? SlotStart,
    DateTimeOffset? SlotEnd,
    Guid? RoomId,
    string? RoomName
);
