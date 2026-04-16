using ConferenceExample.Conference.Domain.SharedKernel;

namespace ConferenceExample.Conference.Domain.TalkManagement.Events;

/// <summary>
/// Domain event that contains the complete state of a Conference aggregate when a Talk is submitted to it.
/// Used both for domain logic and for updating Read Models across bounded contexts.
/// </summary>
public record TalkSubmittedToConferenceEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    long Version,
    // Conference State
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
    // Event-specific data
    Guid TalkId
) : IDomainEvent;
