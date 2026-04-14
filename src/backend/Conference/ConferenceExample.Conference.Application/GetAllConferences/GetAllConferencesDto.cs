namespace ConferenceExample.Conference.Application.GetAllConferences;

public record GetAllConferencesDto(
    Guid Id,
    string Name,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string City,
    string State,
    string PostalCode,
    string Country
);
