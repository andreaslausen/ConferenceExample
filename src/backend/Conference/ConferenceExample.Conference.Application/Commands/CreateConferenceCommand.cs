namespace ConferenceExample.Conference.Application.Commands;

public record CreateConferenceCommand(
    string Name,
    DateTimeOffset Start,
    DateTimeOffset End,
    string LocationName,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country
);
