using ConferenceExample.Talk.Domain.SharedKernel.Extensions;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using MongoDB.Driver;

namespace ConferenceExample.Talk.Persistence.ReadModels;

public class MongoDbSpeakerDirectory : ISpeakerDirectoryDocumentRepository, ISpeakerDirectory
{
    private readonly IMongoCollection<SpeakerDirectoryDocument> _collection;

    public MongoDbSpeakerDirectory(IMongoDatabase database)
    {
        _collection = database.GetCollection<SpeakerDirectoryDocument>("talk_speaker_directory");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var userIndexKeys = Builders<SpeakerDirectoryDocument>.IndexKeys.Ascending(d => d.UserId);
        var userIndexModel = new CreateIndexModel<SpeakerDirectoryDocument>(
            userIndexKeys,
            new CreateIndexOptions { Name = "idx_userId", Unique = true }
        );

        _collection.Indexes.CreateOne(userIndexModel);
    }

    public async Task Save(SpeakerDirectoryDocument document)
    {
        var filter = Builders<SpeakerDirectoryDocument>.Filter.Eq(d => d.Id, document.Id);
        await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true });
    }

    public async Task<SpeakerId?> FindSpeakerIdByUserId(UserId userId)
    {
        var filter = Builders<SpeakerDirectoryDocument>.Filter.Eq(
            d => d.UserId,
            userId.Value.Value.ToString()
        );

        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        return document is null ? null : new SpeakerId(document.Id.ToGuid());
    }
}
