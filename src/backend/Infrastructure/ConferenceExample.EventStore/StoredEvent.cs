using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConferenceExample.EventStore;

public record StoredEvent(
    [property: BsonId] [property: BsonRepresentation(BsonType.String)] Guid Id,
    [property: BsonRepresentation(BsonType.String)] Guid AggregateId,
    string EventType,
    string Payload,
    DateTimeOffset OccurredAt,
    long Version
);
