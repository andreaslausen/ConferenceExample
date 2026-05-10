using ConferenceExample.Talk.Domain.SharedKernel;

namespace ConferenceExample.Talk.Domain.SpeakerManagement.Events;

public record SpeakerProfileUpdatedEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    string FirstName,
    string LastName,
    string Biography
) : IDomainEvent;
