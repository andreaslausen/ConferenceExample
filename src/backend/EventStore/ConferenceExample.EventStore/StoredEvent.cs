namespace ConferenceExample.EventStore;

public record StoredEvent(
    Guid Id,
    Guid AggregateId,
    string EventType,
    string Payload,
    DateTimeOffset OccurredAt,
    long Version
);
