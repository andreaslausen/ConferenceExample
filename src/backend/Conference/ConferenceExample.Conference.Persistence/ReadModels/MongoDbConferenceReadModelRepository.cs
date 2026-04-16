using MongoDB.Driver;

namespace ConferenceExample.Conference.Persistence.ReadModels;

/// <summary>
/// MongoDB implementation of Conference Read Model repository.
/// Stores denormalized conference data in the 'conference_readmodels' collection.
/// </summary>
public class MongoDbConferenceReadModelRepository : IConferenceReadModelRepository
{
    private readonly IMongoCollection<ConferenceReadModel> _collection;

    public MongoDbConferenceReadModelRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ConferenceReadModel>("conference_readmodels");
    }

    public async Task<ConferenceReadModel?> GetById(Guid conferenceId)
    {
        var filter = Builders<ConferenceReadModel>.Filter.Eq(c => c.Id, conferenceId.ToString());
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<ConferenceReadModel>> GetAll()
    {
        var conferences = await _collection
            .Find(FilterDefinition<ConferenceReadModel>.Empty)
            .ToListAsync();
        return conferences;
    }

    public async Task Save(ConferenceReadModel conferenceReadModel)
    {
        await _collection.InsertOneAsync(conferenceReadModel);
    }

    public async Task Update(ConferenceReadModel conferenceReadModel)
    {
        // Optimistic locking: Only update if the event version is newer than the current read model version
        var filter = Builders<ConferenceReadModel>.Filter.And(
            Builders<ConferenceReadModel>.Filter.Eq(c => c.Id, conferenceReadModel.Id),
            Builders<ConferenceReadModel>.Filter.Lt(c => c.Version, conferenceReadModel.Version)
        );

        _ = await _collection.ReplaceOneAsync(filter, conferenceReadModel);

        // If ModifiedCount is 0, the read model already has a newer or equal version
        // This is expected with out-of-order events and can be safely ignored
    }

    public async Task Delete(Guid conferenceId)
    {
        var filter = Builders<ConferenceReadModel>.Filter.Eq(c => c.Id, conferenceId.ToString());
        await _collection.DeleteOneAsync(filter);
    }
}
