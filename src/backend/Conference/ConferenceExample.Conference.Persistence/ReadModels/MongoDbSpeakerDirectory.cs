using MongoDB.Driver;

namespace ConferenceExample.Conference.Persistence.ReadModels;

public class MongoDbSpeakerDirectory : ISpeakerDocumentRepository
{
    private readonly IMongoCollection<SpeakerDocument> _collection;

    public MongoDbSpeakerDirectory(IMongoDatabase database)
    {
        _collection = database.GetCollection<SpeakerDocument>("conference_speaker_directory");
    }

    public async Task<SpeakerDocument?> GetById(Guid speakerId)
    {
        var filter = Builders<SpeakerDocument>.Filter.Eq(s => s.Id, speakerId.ToString());
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task Save(SpeakerDocument document)
    {
        var filter = Builders<SpeakerDocument>.Filter.Eq(s => s.Id, document.Id);
        await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true });
    }
}
