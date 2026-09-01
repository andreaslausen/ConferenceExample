using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

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
