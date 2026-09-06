using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Application.SubmitTalk;

public class SubmitTalkCommandHandler(
    ITalkRepository talkRepository,
    ICurrentUserService currentUserService,
    ISpeakerRepository speakerRepository
) : ISubmitTalkCommandHandler
{
    public async Task<Guid> Handle(SubmitTalkCommand command)
    {
        // Whether the referenced conference exists, and whether it's accepting submissions, are
        // both Conference's own concerns — Talk doesn't keep a local copy of Conference state to
        // check synchronously. Both are enforced asynchronously by Conference.SubmitTalk (or, for
        // a conference that doesn't exist at all, by the handler that reacts to TalkSubmittedEvent
        // on the Conference side) once this talk's TalkSubmittedEvent reaches the Conference BC.
        var currentUserId = currentUserService.GetCurrentUserId();
        var speakerId = new SpeakerId(new GuidV7(currentUserId));

        var speaker = await speakerRepository.GetSpeaker(speakerId);

        var talkId = new TalkId(GuidV7.NewGuid());
        var talk = Domain.TalkManagement.Talk.Submit(
            talkId,
            new TalkTitle(command.Title),
            speakerId,
            speaker.Name.FirstName,
            speaker.Name.LastName,
            speaker.Biography.Content,
            command.Tags.Select(t => new TalkTag(t)),
            new TalkTypeId(command.TalkTypeId),
            new Abstract(command.Abstract),
            new ConferenceId(command.ConferenceId)
        );

        await talkRepository.Save(talk);

        return talkId.Value.Value;
    }
}
