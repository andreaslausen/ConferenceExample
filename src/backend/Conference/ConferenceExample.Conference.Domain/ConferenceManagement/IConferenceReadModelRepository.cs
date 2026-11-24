using ConferenceExample.Conference.Domain.ValueObjects;
using ConferenceExample.Conference.Domain.ValueObjects.Ids;

namespace ConferenceExample.Conference.Domain.ConferenceManagement;

public interface IConferenceReadModelRepository
{
    Task<(IReadOnlyList<ConferenceReadModel> Items, int TotalCount)> GetAll(
        PageRequest pageRequest
    );
    Task<(IReadOnlyList<ConferenceReadModel> Items, int TotalCount)> GetByOrganizerId(
        OrganizerId organizerId,
        PageRequest pageRequest
    );
}
