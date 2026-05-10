using ConferenceExample.EventStore;
using MongoDB.Driver;

namespace ConferenceExample.Conference.Persistence;

public class ConferenceEventStore(IMongoDatabase database, IEventBus eventBus)
    : MongoDbEventStore(database, "conference_events", eventBus),
        IConferenceEventStore { }
