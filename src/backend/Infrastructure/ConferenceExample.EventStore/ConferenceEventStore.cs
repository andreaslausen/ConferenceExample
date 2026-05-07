using MongoDB.Driver;

namespace ConferenceExample.EventStore;

public class ConferenceEventStore(IMongoDatabase database)
    : MongoDbEventStore(database, "conference_events"),
        IConferenceEventStore { }
