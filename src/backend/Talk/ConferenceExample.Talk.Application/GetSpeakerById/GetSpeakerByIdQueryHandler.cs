using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;

namespace ConferenceExample.Talk.Application.GetSpeakerById;

public class GetSpeakerByIdQueryHandler(ISpeakerReadModelRepository speakerReadModelRepository)
    : IGetSpeakerByIdQueryHandler
{
    public async Task<GetSpeakerByIdDto?> Handle(GetSpeakerByIdQuery query)
    {
        var speakerId = new SpeakerId(new GuidV7(query.SpeakerId));
        var profile = await speakerReadModelRepository.GetById(speakerId);

        if (profile is null)
            return null;

        return new GetSpeakerByIdDto(
            profile.Id,
            profile.FirstName,
            profile.LastName,
            profile.Biography
        );
    }
}
