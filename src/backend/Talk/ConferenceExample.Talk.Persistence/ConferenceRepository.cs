using ConferenceExample.Talk.Domain.SharedKernel;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.TalkManagement;
using ConferenceExample.Talk.Persistence.ReadModels;

namespace ConferenceExample.Talk.Persistence;

public class ConferenceRepository(IConferenceStatusDocumentRepository statusDocumentRepository)
    : IConferenceRepository
{
    public async Task<Conference> GetById(ConferenceId conferenceId)
    {
        var aggregateId = conferenceId.Value.Value;
        var document = await statusDocumentRepository.GetById(aggregateId.ToString());

        if (document is null)
        {
            throw new NotFoundException($"Conference with id {conferenceId.Value} does not exist.");
        }

        return Conference.FromEvents(conferenceId);
    }
}
