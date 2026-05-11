using ConferenceExample.Conference.Domain.SharedKernel;

namespace ConferenceExample.Conference.Domain.TalkManagement.Events;

public record TalkScheduledEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    Guid TalkId,
    DateTimeOffset TalkStart,
    DateTimeOffset TalkEnd
) : IDomainEvent;
