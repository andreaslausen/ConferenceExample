using ConferenceExample.Speaker.Domain.SharedKernel.Extensions;
using ConferenceExample.Speaker.Domain.SpeakerManagement;
using MongoDB.Driver;

namespace ConferenceExample.Speaker.Persistence.ReadModels;

public class MongoDbSpeakerReadModelRepository
    : ISpeakerDocumentRepository,
        ISpeakerReadModelRepository,
        ISpeakerLookup
{
    private readonly IMongoCollection<SpeakerDocument> _collection;

    public MongoDbSpeakerReadModelRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<SpeakerDocument>("speaker_readmodels");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var userIndexKeys = Builders<SpeakerDocument>.IndexKeys.Ascending(s => s.UserId);
        var userIndexModel = new CreateIndexModel<SpeakerDocument>(
            userIndexKeys,
            new CreateIndexOptions { Name = "idx_userId", Unique = true }
        );

        _collection.Indexes.CreateOne(userIndexModel);
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
        return ToReadModel(await _collection.Find(filter).FirstOrDefaultAsync());
    }

    async Task<SpeakerReadModel?> ISpeakerReadModelRepository.GetByUserId(UserId userId)
    {
        return ToReadModel(await FindByUserId(userId));
    }

    async Task<SpeakerId?> ISpeakerLookup.FindSpeakerIdByUserId(UserId userId)
    {
        var document = await FindByUserId(userId);
        return document is null ? null : new SpeakerId(document.Id.ToGuid());
    }

    private async Task<SpeakerDocument?> FindByUserId(UserId userId)
    {
        var filter = Builders<SpeakerDocument>.Filter.Eq(
            s => s.UserId,
            userId.Value.Value.ToString()
        );
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    private static SpeakerReadModel? ToReadModel(SpeakerDocument? document) =>
        document is null
            ? null
            : new SpeakerReadModel(
                document.Id.ToGuid(),
                document.UserId.ToGuid(),
                document.FirstName,
                document.LastName,
                document.Biography
            );

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
