using ConferenceExample.Talk.Domain.ConferenceManagement;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Application.SubmitTalkToConference;

public class SubmitTalkToConferenceCommandHandler(
    ITalkRepository talkRepository,
    ICurrentSpeakerProvider currentSpeakerProvider
) : ISubmitTalkToConferenceCommandHandler
{
    public async Task Handle(SubmitTalkToConferenceCommand command)
    {
        // Whether the conference exists, is accepting submissions, and knows the requested talk
        // type are all the conference's own concerns — the Talk BC keeps no copy of that state to
        // check synchronously. Conference decides asynchronously once this talk's
        // TalkSubmittedToConferenceEvent reaches it, and reports back with either
        // TalkSubmissionRegisteredEvent or TalkSubmissionRejectedEvent.
        var talk = await talkRepository.GetById(new TalkId(new GuidV7(command.TalkId)));

        var currentSpeakerId = await currentSpeakerProvider.GetCurrentSpeakerId();

        if (talk.SpeakerId != currentSpeakerId)
        {
            throw new UnauthorizedAccessException(
                $"Speaker {currentSpeakerId.Value} is not authorized to submit talk {talk.Id.Value}."
            );
        }

        talk.SubmitToConference(
            new ConferenceId(new GuidV7(command.ConferenceId)),
            new TalkTypeId(new GuidV7(command.TalkTypeId))
        );

        await talkRepository.Save(talk);
    }
}
