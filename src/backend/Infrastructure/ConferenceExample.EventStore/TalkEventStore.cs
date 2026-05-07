using MongoDB.Driver;

namespace ConferenceExample.EventStore;

public class TalkEventStore(IMongoDatabase database)
    : MongoDbEventStore(database, "talk_events"),
        ITalkEventStore { }
