using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Application.SubmitTalk;

public class SubmitTalkCommandHandler(
    ITalkRepository talkRepository,
    ICurrentUserService currentUserService,
    IConferenceRepository conferenceRepository,
    ISpeakerRepository speakerRepository
) : ISubmitTalkCommandHandler
{
    public async Task<Guid> Handle(SubmitTalkCommand command)
    {
        var conferenceId = new ConferenceId(new GuidV7(command.ConferenceId));
        var conference = await conferenceRepository.GetById(conferenceId);

        if (!conference.CanAcceptTalkSubmissions())
        {
            throw new InvalidOperationException(
                $"Talks can only be submitted when the conference is in CallForSpeakers status. Current status: {conference.Status}"
            );
        }

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
