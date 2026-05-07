using MongoDB.Driver;

namespace ConferenceExample.EventStore.UnitTests;

internal class TestEventStore(IMongoDatabase database)
    : MongoDbEventStore(database, "test_events") { }
