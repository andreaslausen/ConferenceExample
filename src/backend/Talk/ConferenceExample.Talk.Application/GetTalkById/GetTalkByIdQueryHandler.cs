using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Application.GetTalkById;

/// <summary>
/// A talk is the speaker's own working material, so only its owner can read it here. Organizers
/// see what was submitted to their conference — a snapshot the Conference BC owns — through the
/// conference's own endpoints, never through this one.
/// </summary>
public class GetTalkByIdQueryHandler(
    ITalkReadModelRepository talkReadModelRepository,
    ICurrentSpeakerProvider currentSpeakerProvider
) : IGetTalkByIdQueryHandler
{
    public async Task<GetTalkByIdDto?> Handle(GetTalkByIdQuery query)
    {
        var talk = await talkReadModelRepository.GetById(new TalkId(new GuidV7(query.TalkId)));

        if (talk is null)
            return null;

        var currentSpeakerId = await currentSpeakerProvider.GetCurrentSpeakerId();

        if (new SpeakerId(new GuidV7(talk.SpeakerId)) != currentSpeakerId)
            return null;

        return new GetTalkByIdDto(
            talk.Id,
            talk.Title,
            talk.Abstract,
            talk.SpeakerId,
            talk.Tags.ToList(),
            talk.SubmissionCount
        );
    }
}
