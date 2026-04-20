using ConferenceExample.Talk.Domain.SharedKernel.Extensions;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using MongoDB.Driver;

namespace ConferenceExample.Talk.Persistence.ReadModels;

public class MongoDbSpeakerReadModelRepository
    : ISpeakerDocumentRepository,
        ISpeakerReadModelRepository
{
    private readonly IMongoCollection<SpeakerDocument> _collection;

    public MongoDbSpeakerReadModelRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<SpeakerDocument>("speaker_readmodels");
    }

    public async Task<SpeakerDocument?> GetById(Guid speakerId)
    {
        var filter = Builders<SpeakerDocument>.Filter.Eq(s => s.Id, speakerId.ToString());
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    async Task<SpeakerReadModel?> ISpeakerReadModelRepository.GetById(SpeakerId speakerId)
    {
        var filter = Builders<SpeakerDocument>.Filter.Eq(
            s => s.Id,
            speakerId.Value.Value.ToString()
        );
        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        if (document is null)
            return null;

        return new SpeakerReadModel(
            document.Id.ToGuid(),
            document.FirstName,
            document.LastName,
            document.Biography
        );
    }

    public async Task Save(SpeakerDocument document)
    {
        await _collection.InsertOneAsync(document);
    }

    public async Task Update(SpeakerDocument document)
    {
        var filter = Builders<SpeakerDocument>.Filter.And(
            Builders<SpeakerDocument>.Filter.Eq(s => s.Id, document.Id),
            Builders<SpeakerDocument>.Filter.Lt(s => s.Version, document.Version)
        );

        _ = await _collection.ReplaceOneAsync(filter, document);
    }
}
