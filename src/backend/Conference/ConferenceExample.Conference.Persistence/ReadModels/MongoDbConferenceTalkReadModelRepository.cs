using MongoDB.Driver;

namespace ConferenceExample.Conference.Persistence.ReadModels;

/// <summary>
/// MongoDB implementation of Talk Read Model repository for the Conference bounded context.
/// Stores denormalized talk data replicated from Talk BC in the 'conference_talk_readmodels' collection.
/// </summary>
public class MongoDbConferenceTalkReadModelRepository : IConferenceTalkReadModelRepository
{
    private readonly IMongoCollection<ConferenceTalkReadModel> _collection;

    public MongoDbConferenceTalkReadModelRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ConferenceTalkReadModel>("conference_talk_readmodels");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        // Index for conference queries
        var conferenceIndexKeys = Builders<ConferenceTalkReadModel>.IndexKeys.Ascending(t =>
            t.ConferenceId
        );
        var conferenceIndexModel = new CreateIndexModel<ConferenceTalkReadModel>(
            conferenceIndexKeys,
            new CreateIndexOptions { Name = "idx_conferenceId" }
        );

        _collection.Indexes.CreateOne(conferenceIndexModel);
    }

    public async Task<ConferenceTalkReadModel?> GetById(Guid talkId)
    {
        var filter = Builders<ConferenceTalkReadModel>.Filter.Eq(t => t.Id, talkId.ToString());
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<ConferenceTalkReadModel>> GetByConferenceId(Guid conferenceId)
    {
        var filter = Builders<ConferenceTalkReadModel>.Filter.Eq(
            t => t.ConferenceId,
            conferenceId.ToString()
        );
        var talks = await _collection.Find(filter).ToListAsync();
        return talks;
    }

    public async Task Save(ConferenceTalkReadModel talkReadModel)
    {
        await _collection.InsertOneAsync(talkReadModel);
    }

    public async Task Update(ConferenceTalkReadModel talkReadModel)
    {
        // Optimistic locking: Only update if the event version is newer than the current read model version
        var filter = Builders<ConferenceTalkReadModel>.Filter.And(
            Builders<ConferenceTalkReadModel>.Filter.Eq(t => t.Id, talkReadModel.Id),
            Builders<ConferenceTalkReadModel>.Filter.Lt(t => t.Version, talkReadModel.Version)
        );

        _ = await _collection.ReplaceOneAsync(filter, talkReadModel);

        // If ModifiedCount is 0, the read model already has a newer or equal version
        // This is expected with out-of-order events and can be safely ignored
    }

    public async Task Delete(Guid talkId)
    {
        var filter = Builders<ConferenceTalkReadModel>.Filter.Eq(t => t.Id, talkId.ToString());
        await _collection.DeleteOneAsync(filter);
    }
}
