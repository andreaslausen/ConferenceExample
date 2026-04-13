# MongoDB Event Store Setup

This document explains the MongoDB-based Event Store implementation and how to use it.

## Architecture

The system uses **Event Sourcing** with MongoDB as the persistence layer:

- **MongoDbEventStore** - Stores events in MongoDB with optimistic concurrency control
- **MongoDbEventBus** - Uses MongoDB Change Streams for real-time event notifications

## Prerequisites

- Docker and Docker Compose
- .NET 10 SDK
- MongoDB.Driver NuGet package (already included)

## Quick Start

### 1. Start MongoDB

```bash
docker-compose up -d
```

This starts:
- **MongoDB 8.0** on `localhost:27017` (configured as replica set for Change Streams)
- **Mongo Express** on `localhost:8081` (web UI for MongoDB)

### 2. Configure the Application

The application uses `appsettings.Development.json` by default in Development environment:

```json
{
  "EventStore": {
    "MongoDB": {
      "ConnectionString": "mongodb://admin:admin123@localhost:27017",
      "DatabaseName": "conference_events"
    }
  }
}
```

**Configuration Options:**
- `ConnectionString`: MongoDB connection string with authentication
- `DatabaseName`: Name of the database for event storage

### 3. Run the Application

```bash
dotnet run --project src/backend/API/ConferenceExample.API
```

## MongoDB Collections

### `events` Collection

Structure:
```json
{
  "_id": "ObjectId(...)",
  "Id": "GUID",
  "AggregateId": "GUID", 
  "EventType": "ConferenceCreatedEvent",
  "Payload": "{...JSON...}",
  "OccurredAt": "ISODate(...)",
  "Version": 0
}
```

### Indexes

- `idx_aggregate_version`: Compound index on `(AggregateId, Version)` for fast aggregate retrieval
- `idx_version`: Index on `Version` for ordered event replay
- `idx_id`: Unique index on `Id` to prevent duplicates
- `idx_event_type`: Index on `EventType` for event filtering
- `idx_occurred_at`: Index on `OccurredAt` for time-based queries

## Features

### Optimistic Concurrency Control

Events are appended atomically using MongoDB transactions:

```csharp
// Check expected version matches current version
var currentVersion = await GetCurrentVersion(aggregateId);
if (currentVersion != expectedVersion) {
    throw new ConcurrencyException(...);
}

// Insert events atomically
await collection.InsertManyAsync(events);
```

### Change Streams (Real-time Event Bus)

MongoDB Change Streams provide real-time notifications when events are inserted:

```csharp
var changeStream = collection.Watch();
await changeStream.ForEachAsync(change => {
    NotifySubscribers(change.FullDocument);
});
```

**Important Notes:**

- **Replica Set Required**: Change Streams require MongoDB to be configured as a **replica set**. The Docker Compose setup handles this automatically.

- **Multi-Instance Support**: Change Streams ensure ALL application instances receive ALL events
  - Events are propagated only via Change Streams (not immediately on `Publish()`)
  - This guarantees consistent behavior across all instances
  - Small latency (5-20ms) is acceptable for cross-instance correctness

- **Single-Instance Deployment**: Even with a single instance, Change Streams are used for consistency
  - Minimal overhead (5-20ms per event)
  - Same code path for single and multi-instance deployments

### Event Replay

Aggregates are reconstructed by replaying their events:

```csharp
var events = await eventStore.GetEvents(aggregateId);
var aggregate = Conference.LoadFromHistory(events);
```

## Web UI

Access Mongo Express at `http://localhost:8081` to:
- Browse the `conference_events` database
- View events in the `events` collection
- Debug event payloads
- Monitor Change Streams

## Production Considerations

### Scaling

MongoDB handles millions of events easily:

- **Sharding**: Distribute events across multiple nodes by `AggregateId`
- **Storage**: BSON compression reduces storage by 30-50%
- **Performance**: 15k-20k writes/sec on single node, 100k+ with sharding

### Backup

```bash
# Dump database
docker exec conference-mongodb mongodump --db conference_events --out /dump

# Restore database
docker exec conference-mongodb mongorestore --db conference_events /dump/conference_events
```

### Replica Set

For production, configure a proper replica set with multiple nodes:

```yaml
services:
  mongodb1:
    image: mongo:8.0
    command: --replSet rs0
  mongodb2:
    image: mongo:8.0
    command: --replSet rs0
  mongodb3:
    image: mongo:8.0
    command: --replSet rs0
```

## Troubleshooting

### Change Streams not working

If you see: `MongoDB Change Streams not available (requires replica set)`

**Solution**: Make sure MongoDB is running as a replica set:

```bash
docker-compose down
docker-compose up -d
```

Wait for MongoDB to initialize the replica set (check logs):
```bash
docker logs conference-mongodb
```

### Connection refused

Make sure MongoDB is running:
```bash
docker-compose ps
```

Check MongoDB logs:
```bash
docker logs conference-mongodb
```

## Testing

Run integration tests against MongoDB:

```bash
# Start MongoDB
docker-compose up -d

# Run tests
dotnet test src/backend/Infrastructure/ConferenceExample.EventStore.UnitTests
```

## Further Reading

- [MongoDB Event Sourcing](https://www.mongodb.com/blog/post/event-sourcing-with-mongodb)
- [Change Streams Documentation](https://www.mongodb.com/docs/manual/changeStreams/)
- [Event Sourcing Pattern](https://martinfowler.com/eaaDev/EventSourcing.html)
