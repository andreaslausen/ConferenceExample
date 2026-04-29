namespace ConferenceExample.Conference.Application.UpdateConferenceDetails;

public record UpdateConferenceDetailsCommand(
    Guid Id,
    string Name,
    DateTimeOffset? Start,
    DateTimeOffset? End,
    string LocationName,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country
);
