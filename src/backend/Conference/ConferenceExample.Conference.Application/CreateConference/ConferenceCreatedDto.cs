namespace ConferenceExample.Conference.Application.CreateConference;

public record ConferenceCreatedDto(
    Guid Id,
    string Name,
    DateTimeOffset Start,
    DateTimeOffset End,
    string LocationName
);
