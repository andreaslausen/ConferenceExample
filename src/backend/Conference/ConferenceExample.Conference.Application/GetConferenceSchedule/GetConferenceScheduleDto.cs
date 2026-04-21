namespace ConferenceExample.Conference.Application.GetConferenceSchedule;

public record GetConferenceScheduleDto(
    Guid Id,
    string Title,
    string Status,
    DateTimeOffset? SlotStart,
    DateTimeOffset? SlotEnd,
    Guid? RoomId,
    string? RoomName
);
