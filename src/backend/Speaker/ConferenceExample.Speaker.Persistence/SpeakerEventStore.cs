using ConferenceExample.EventStore;
using MongoDB.Driver;

namespace ConferenceExample.Speaker.Persistence;

public class SpeakerEventStore(IMongoDatabase database, IEventBus eventBus)
    : MongoDbEventStore(database, "speaker_events", eventBus),
        ISpeakerEventStore { }
