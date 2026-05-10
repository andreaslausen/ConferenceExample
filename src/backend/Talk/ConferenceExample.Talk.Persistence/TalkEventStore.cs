using ConferenceExample.EventStore;
using MongoDB.Driver;

namespace ConferenceExample.Talk.Persistence;

public class TalkEventStore(IMongoDatabase database, IEventBus eventBus)
    : MongoDbEventStore(database, "talk_events", eventBus),
        ITalkEventStore { }
