using ConferenceExample.Conference.Domain.SharedKernel;

namespace ConferenceExample.Conference.Domain.ConferenceManagement.Events;

public record TalkTypeRemovedEvent(Guid AggregateId, DateTimeOffset OccurredAt, Guid TalkTypeId)
    : IDomainEvent;
