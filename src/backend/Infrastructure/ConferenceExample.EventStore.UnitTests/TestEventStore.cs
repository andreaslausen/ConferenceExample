using MongoDB.Driver;

namespace ConferenceExample.EventStore.UnitTests;

internal class TestEventStore(IMongoDatabase database, IEventBus eventBus)
    : MongoDbEventStore(database, "test_events", eventBus) { }
