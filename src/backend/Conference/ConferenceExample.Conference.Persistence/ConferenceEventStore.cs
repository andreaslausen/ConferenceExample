using ConferenceExample.EventStore;
using MongoDB.Driver;

namespace ConferenceExample.Conference.Persistence;

public class ConferenceEventStore(IMongoDatabase database)
    : MongoDbEventStore(database, "conference_events"),
        IConferenceEventStore { }
