using ConferenceExample.Conference.Domain.SharedKernel;

namespace ConferenceExample.Conference.Domain.ConferenceManagement.Events;

public record TalkTypeDefinedEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    Guid TalkTypeId,
    string TalkTypeName,
    int TalkTypeDurationInMinutes
) : IDomainEvent;
