using MongoDB.Driver;

namespace ConferenceExample.Talk.Persistence.ReadModels;

/// <summary>
/// MongoDB implementation of Talk Read Model repository.
/// Stores denormalized talk data in the 'talk_readmodels' collection.
/// </summary>
public class MongoDbTalkReadModelRepository : ITalkReadModelRepository
{
    private readonly IMongoCollection<TalkReadModel> _collection;

    public MongoDbTalkReadModelRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<TalkReadModel>("talk_readmodels");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        // Index for conference queries
        var conferenceIndexKeys = Builders<TalkReadModel>.IndexKeys.Ascending(t => t.ConferenceId);
        var conferenceIndexModel = new CreateIndexModel<TalkReadModel>(
            conferenceIndexKeys,
            new CreateIndexOptions { Name = "idx_conferenceId" }
        );

        // Index for speaker queries
        var speakerIndexKeys = Builders<TalkReadModel>.IndexKeys.Ascending(t => t.SpeakerId);
        var speakerIndexModel = new CreateIndexModel<TalkReadModel>(
            speakerIndexKeys,
            new CreateIndexOptions { Name = "idx_speakerId" }
        );

        _collection.Indexes.CreateMany(new[] { conferenceIndexModel, speakerIndexModel });
    }

    public async Task<TalkReadModel?> GetById(Guid talkId)
    {
        var filter = Builders<TalkReadModel>.Filter.Eq(t => t.Id, talkId.ToString());
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<TalkReadModel>> GetByConferenceId(Guid conferenceId)
    {
        var filter = Builders<TalkReadModel>.Filter.Eq(
            t => t.ConferenceId,
            conferenceId.ToString()
        );
        var talks = await _collection.Find(filter).ToListAsync();
        return talks;
    }

    public async Task<IReadOnlyList<TalkReadModel>> GetBySpeakerId(Guid speakerId)
    {
        var filter = Builders<TalkReadModel>.Filter.Eq(t => t.SpeakerId, speakerId.ToString());
        var talks = await _collection.Find(filter).ToListAsync();
        return talks;
    }

    public async Task Save(TalkReadModel talkReadModel)
    {
        await _collection.InsertOneAsync(talkReadModel);
    }

    public async Task Update(TalkReadModel talkReadModel)
    {
        var filter = Builders<TalkReadModel>.Filter.Eq(t => t.Id, talkReadModel.Id);
        await _collection.ReplaceOneAsync(filter, talkReadModel);
    }

    public async Task Delete(Guid talkId)
    {
        var filter = Builders<TalkReadModel>.Filter.Eq(t => t.Id, talkId.ToString());
        await _collection.DeleteOneAsync(filter);
    }
}
