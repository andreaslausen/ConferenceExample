using ConferenceExample.Talk.Domain.ConferenceManagement;
using ConferenceExample.Talk.Domain.SharedKernel.Extensions;
using MongoDB.Driver;

namespace ConferenceExample.Talk.Persistence.ReadModels;

public class MongoDbConferenceDirectory : IConferenceDocumentRepository, IConferenceDirectory
{
    private readonly IMongoCollection<ConferenceDocument> _collection;

    public MongoDbConferenceDirectory(IMongoDatabase database)
    {
        _collection = database.GetCollection<ConferenceDocument>("talk_conference_directory");
    }

    public async Task Save(ConferenceDocument document)
    {
        var filter = Builders<ConferenceDocument>.Filter.Eq(d => d.Id, document.Id);
        await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true });
    }

    public async Task UpdateName(Guid conferenceId, string name)
    {
        var filter = Builders<ConferenceDocument>.Filter.Eq(d => d.Id, conferenceId.ToString());
        var update = Builders<ConferenceDocument>.Update.Set(d => d.Name, name);
        await _collection.UpdateOneAsync(filter, update);
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetConferenceNames(
        IReadOnlyCollection<ConferenceId> conferenceIds
    )
    {
        if (conferenceIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var ids = conferenceIds.Select(id => id.Value.Value.ToString()).ToList();
        var filter = Builders<ConferenceDocument>.Filter.In(d => d.Id, ids);

        var documents = await _collection.Find(filter).ToListAsync();

        return documents.ToDictionary(d => d.Id.ToGuid(), d => d.Name);
    }
}
