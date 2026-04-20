namespace ConferenceExample.Conference.Domain.ConferenceManagement;

public record ConferenceReadModel(
    Guid Id,
    string Name,
    DateTimeOffset Start,
    DateTimeOffset End,
    string City,
    string State,
    string PostalCode,
    string Country,
    string Status
);
