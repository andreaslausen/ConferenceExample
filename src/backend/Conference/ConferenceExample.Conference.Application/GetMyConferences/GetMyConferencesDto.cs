namespace ConferenceExample.Conference.Application.GetMyConferences;

public record GetMyConferencesDto(
    Guid Id,
    string Name,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string City,
    string State,
    string PostalCode,
    string Country,
    string Status
);
