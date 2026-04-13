using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Domain.ConferenceManagement;

public interface IConferenceRepository
{
    Task<Conference> GetById(ConferenceId id);
    Task Save(Conference conference);
}
