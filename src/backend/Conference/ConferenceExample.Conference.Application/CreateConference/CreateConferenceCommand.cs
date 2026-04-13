namespace ConferenceExample.Conference.Application.CreateConference;

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
