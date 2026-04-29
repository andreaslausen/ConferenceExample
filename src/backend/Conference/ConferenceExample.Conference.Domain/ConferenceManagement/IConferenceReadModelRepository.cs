using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Domain.ConferenceManagement;

public interface IConferenceReadModelRepository
{
    Task<IReadOnlyList<ConferenceReadModel>> GetAll();
    Task<IReadOnlyList<ConferenceReadModel>> GetByOrganizerId(OrganizerId organizerId);
}
