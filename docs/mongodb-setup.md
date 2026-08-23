# MongoDB Event Store Setup

This document explains the MongoDB-based Event Store implementation and how to use it.

## Architecture

The system uses **Event Sourcing** with MongoDB as the persistence layer:

- **MongoDbEventStore** - Stores events in MongoDB with optimistic concurrency control
- **InMemoryEventBus** - Publishes events in-process (FIFO, deduplicated) after a successful
  append; not durable across restarts and not shared across instances (suitable for the
  demo — replace with RabbitMQ/Kafka in production)

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
- **MongoDB 8.0** on `localhost:27017` (configured as replica set — required for the
  multi-document transactions used by optimistic concurrency control on append)
- **Mongo Express** on `localhost:8081` (web UI for MongoDB)

### 2. Configure the Application

The application uses `appsettings.Development.json` by default in Development environment:

```json
{
  "Database": {
    "MongoDB": {
      "ConnectionString": "mongodb://admin:admin123@localhost:27017",
      "DatabaseName": "conference_example"
    }
  }
}
```

**Configuration Options:**
- `ConnectionString`: MongoDB connection string with authentication
- `DatabaseName`: Name of the database for event storage and read models

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

### Event Bus (In-Process Notifications)

After events are appended, `MongoDbEventStore` publishes them via `IEventBus`
(`InMemoryEventBus`), which fans them out to subscribed handlers in FIFO order:

```csharp
eventBus.Subscribe(nameof(ConferenceCreatedEvent), async storedEvent => {
    // update read models, etc.
});
eventBus.Publish(storedEvent);
```

**Important Notes:**

- **Single process only**: `InMemoryEventBus` runs in-process. In a multi-instance
  deployment, only the instance that appended the event dispatches it — other instances do
  not receive it. This is fine for the demo's single-instance setup; a production
  deployment needs a shared bus (RabbitMQ/Kafka) or MongoDB Change Streams for
  cross-instance propagation.
- **Not durable**: events published while no process is running (or lost mid-flight) are
  not redelivered — read models are rebuilt by replaying the event store, not by replaying
  the bus.

### Event Replay

Aggregates are reconstructed by replaying their events:

```csharp
var events = await eventStore.GetEvents(aggregateId);
var aggregate = Conference.LoadFromHistory(events);
```

## Web UI

Access Mongo Express at `http://localhost:8081` to:
- Browse the `conference_example` database
- View events in the `events` collection
- Debug event payloads

## Production Considerations

### Scaling

MongoDB handles millions of events easily:

- **Sharding**: Distribute events across multiple nodes by `AggregateId`
- **Storage**: BSON compression reduces storage by 30-50%
- **Performance**: 15k-20k writes/sec on single node, 100k+ with sharding

### Backup

```bash
# Dump database
docker exec conference-mongodb mongodump --db conference_example --out /dump

# Restore database
docker exec conference-mongodb mongorestore --db conference_example /dump/conference_example
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

### Transactions failing / "Transaction numbers are only allowed on a replica set member"

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
- [Event Sourcing Pattern](https://martinfowler.com/eaaDev/EventSourcing.html)
