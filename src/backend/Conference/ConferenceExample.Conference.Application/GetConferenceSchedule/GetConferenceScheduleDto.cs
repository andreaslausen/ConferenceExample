namespace ConferenceExample.Conference.Application.GetConferenceSchedule;

public record GetConferenceScheduleDto(
    Guid Id,
    string Status,
    DateTimeOffset? SlotStart,
    DateTimeOffset? SlotEnd,
    Guid? RoomId,
    string? RoomName
);
