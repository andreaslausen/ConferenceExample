// MongoDB initialization script for ConferenceExample Event Store

db = db.getSiblingDB('conference_example');

// Create the events collection
db.createCollection('events');

// Create indexes for optimal query performance
db.events.createIndex({ "AggregateId": 1, "Version": 1 }, { name: "idx_aggregate_version" });
db.events.createIndex({ "Version": 1 }, { name: "idx_version" });
// Note: The Id field is mapped to _id via [BsonId] attribute, so MongoDB automatically creates a unique index on _id
// No need to create a separate index on "Id" field
db.events.createIndex({ "EventType": 1 }, { name: "idx_event_type" });
db.events.createIndex({ "OccurredAt": 1 }, { name: "idx_occurred_at" });

print('ConferenceExample Event Store initialized successfully');
