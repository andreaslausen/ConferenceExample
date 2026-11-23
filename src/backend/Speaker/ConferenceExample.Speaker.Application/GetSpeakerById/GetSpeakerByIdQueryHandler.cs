using ConferenceExample.Speaker.Domain.SpeakerManagement;
using ConferenceExample.Speaker.Domain.ValueObjects.Ids;

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
