using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.Extensions;
using MongoDB.Driver;

namespace ConferenceExample.Conference.Persistence.ReadModels;

public class MongoDbConferenceReadModelRepository
    : IConferenceDocumentRepository,
        IConferenceReadModelRepository
{
    private readonly IMongoCollection<ConferenceDocument> _collection;

    public MongoDbConferenceReadModelRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ConferenceDocument>("conference_readmodels");
    }

    public async Task<ConferenceDocument?> GetById(Guid conferenceId)
    {
        var filter = Builders<ConferenceDocument>.Filter.Eq(c => c.Id, conferenceId.ToString());
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<ConferenceDocument>> GetAll()
    {
        return await _collection.Find(FilterDefinition<ConferenceDocument>.Empty).ToListAsync();
    }

    async Task<IReadOnlyList<ConferenceReadModel>> IConferenceReadModelRepository.GetAll()
    {
        var documents = await _collection
            .Find(FilterDefinition<ConferenceDocument>.Empty)
            .ToListAsync();

        return documents
            .Select(d => new ConferenceReadModel(
                d.Id.ToGuid(),
                d.Name,
                d.Start,
                d.End,
                d.City,
                d.State,
                d.PostalCode,
                d.Country
            ))
            .ToList();
    }

    public async Task Save(ConferenceDocument conferenceDocument)
    {
        await _collection.InsertOneAsync(conferenceDocument);
    }

    public async Task Update(ConferenceDocument conferenceDocument)
    {
        var filter = Builders<ConferenceDocument>.Filter.And(
            Builders<ConferenceDocument>.Filter.Eq(c => c.Id, conferenceDocument.Id),
            Builders<ConferenceDocument>.Filter.Lt(c => c.Version, conferenceDocument.Version)
        );

        _ = await _collection.ReplaceOneAsync(filter, conferenceDocument);
    }

    public async Task Delete(Guid conferenceId)
    {
        var filter = Builders<ConferenceDocument>.Filter.Eq(c => c.Id, conferenceId.ToString());
        await _collection.DeleteOneAsync(filter);
    }
}
