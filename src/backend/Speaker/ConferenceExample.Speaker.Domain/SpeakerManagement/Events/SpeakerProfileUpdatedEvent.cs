using ConferenceExample.Speaker.Domain;

namespace ConferenceExample.Speaker.Domain.SpeakerManagement.Events;

public record SpeakerProfileUpdatedEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    string FirstName,
    string LastName,
    string Biography
) : IDomainEvent;
