using ConferenceExample.Talk.Domain.Extensions;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using MongoDB.Driver;

namespace ConferenceExample.Talk.Persistence.Projections;

public class MongoDbSpeakerReplica : ISpeakerReplicaDocumentRepository, ISpeakerReplica
{
    private readonly IMongoCollection<SpeakerReplicaDocument> _collection;

    public MongoDbSpeakerReplica(IMongoDatabase database)
    {
        _collection = database.GetCollection<SpeakerReplicaDocument>("talk_speaker_replica");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var userIndexKeys = Builders<SpeakerReplicaDocument>.IndexKeys.Ascending(d => d.UserId);
        var userIndexModel = new CreateIndexModel<SpeakerReplicaDocument>(
            userIndexKeys,
            new CreateIndexOptions { Name = "idx_userId", Unique = true }
        );

        _collection.Indexes.CreateOne(userIndexModel);
    }

    public async Task Save(SpeakerReplicaDocument document)
    {
        var filter = Builders<SpeakerReplicaDocument>.Filter.Eq(d => d.Id, document.Id);
        await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true });
    }

    public async Task<SpeakerId?> FindSpeakerIdByUserId(UserId userId)
    {
        var filter = Builders<SpeakerReplicaDocument>.Filter.Eq(
            d => d.UserId,
            userId.Value.Value.ToString()
        );

        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        return document is null ? null : new SpeakerId(document.Id.ToGuid());
    }
}
