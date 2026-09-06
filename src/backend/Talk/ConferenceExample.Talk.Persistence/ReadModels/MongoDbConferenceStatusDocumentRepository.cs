using MongoDB.Driver;

namespace ConferenceExample.Talk.Persistence.ReadModels;

public class MongoDbConferenceStatusDocumentRepository : IConferenceStatusDocumentRepository
{
    private readonly IMongoCollection<ConferenceStatusDocument> _collection;

    public MongoDbConferenceStatusDocumentRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ConferenceStatusDocument>(
            "talk_conference_status_projections"
        );
    }

    public async Task Save(ConferenceStatusDocument document)
    {
        var filter = Builders<ConferenceStatusDocument>.Filter.Eq(d => d.Id, document.Id);
        await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true });
    }

    public async Task<ConferenceStatusDocument?> GetById(string conferenceId)
    {
        var filter = Builders<ConferenceStatusDocument>.Filter.Eq(d => d.Id, conferenceId);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }
}
