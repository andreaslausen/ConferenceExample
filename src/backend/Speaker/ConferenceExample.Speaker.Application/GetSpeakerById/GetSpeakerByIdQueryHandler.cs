using ConferenceExample.Speaker.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Speaker.Domain.SpeakerManagement;

namespace ConferenceExample.Speaker.Application.GetSpeakerById;

public class GetSpeakerByIdQueryHandler(ISpeakerReadModelRepository speakerReadModelRepository)
    : IGetSpeakerByIdQueryHandler
{
    public async Task<GetSpeakerByIdDto?> Handle(GetSpeakerByIdQuery query)
    {
        var speaker = await speakerReadModelRepository.GetById(
            new SpeakerId(new GuidV7(query.SpeakerId))
        );

        if (speaker is null)
            return null;

        return new GetSpeakerByIdDto(
            speaker.Id,
            speaker.FirstName,
            speaker.LastName,
            speaker.Biography
        );
    }
}
