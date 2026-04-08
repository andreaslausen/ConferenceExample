namespace ConferenceExample.Conference.Domain.Events;

public record ConferenceCreatedEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    string Name,
    DateTimeOffset Start,
    DateTimeOffset End,
    string LocationName,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country
) : IDomainEvent;
