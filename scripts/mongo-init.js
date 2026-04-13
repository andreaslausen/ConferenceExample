// MongoDB initialization script for ConferenceExample Event Store

db = db.getSiblingDB('conference_events');

// Create the events collection
db.createCollection('events');

// Create indexes for optimal query performance
db.events.createIndex({ "AggregateId": 1, "Version": 1 }, { name: "idx_aggregate_version" });
db.events.createIndex({ "Version": 1 }, { name: "idx_version" });
db.events.createIndex({ "Id": 1 }, { unique: true, name: "idx_id" });
db.events.createIndex({ "EventType": 1 }, { name: "idx_event_type" });
db.events.createIndex({ "OccurredAt": 1 }, { name: "idx_occurred_at" });

print('ConferenceExample Event Store initialized successfully');
