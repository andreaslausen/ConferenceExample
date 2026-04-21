using ConferenceExample.Conference.Domain.SharedKernel;

namespace ConferenceExample.Conference.Domain.ConferenceManagement.Events;

public record RoomRemovedEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    long Version,
    string Name,
    DateTimeOffset Start,
    DateTimeOffset End,
    string LocationName,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country,
    Guid OrganizerId,
    string Status,
    Guid RoomId
) : IDomainEvent;
