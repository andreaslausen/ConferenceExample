namespace ConferenceExample.Conference.Application.Dtos;

public record ConferenceCreatedDto(
    Guid Id,
    string Name,
    DateTimeOffset Start,
    DateTimeOffset End,
    string LocationName
);
