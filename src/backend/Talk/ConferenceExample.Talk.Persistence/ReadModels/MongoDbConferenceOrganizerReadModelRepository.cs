using ConferenceExample.Talk.Domain.ConferenceManagement;
using ConferenceExample.Talk.Domain.SharedKernel.Extensions;
using MongoDB.Driver;

namespace ConferenceExample.Talk.Persistence.ReadModels;

public class MongoDbConferenceOrganizerReadModelRepository
    : IConferenceOrganizerDocumentRepository,
        IConferenceOrganizerReadModelRepository
{
    private readonly IMongoCollection<ConferenceOrganizerDocument> _collection;

    public MongoDbConferenceOrganizerReadModelRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ConferenceOrganizerDocument>(
            "conference_organizer_readmodels"
        );
    }

    public async Task Save(ConferenceOrganizerDocument document)
    {
        var filter = Builders<ConferenceOrganizerDocument>.Filter.Eq(d => d.Id, document.Id);
        await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true });
    }

    async Task<ConferenceOrganizerReadModel?> IConferenceOrganizerReadModelRepository.GetByConferenceId(
        Guid conferenceId
    )
    {
        var filter = Builders<ConferenceOrganizerDocument>.Filter.Eq(
            d => d.Id,
            conferenceId.ToString()
        );
        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        if (document is null)
            return null;

        return new ConferenceOrganizerReadModel(
            document.Id.ToGuid(),
            document.OrganizerId.ToGuid()
        );
    }
}
