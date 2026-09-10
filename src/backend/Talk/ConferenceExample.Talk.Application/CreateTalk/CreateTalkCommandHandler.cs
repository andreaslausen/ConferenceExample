using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Application.CreateTalk;

public class CreateTalkCommandHandler(
    ITalkRepository talkRepository,
    ICurrentSpeakerProvider currentSpeakerProvider
) : ICreateTalkCommandHandler
{
    public async Task<Guid> Handle(CreateTalkCommand command)
    {
        var speakerId = await currentSpeakerProvider.GetCurrentSpeakerId();

        var talkId = new TalkId(GuidV7.NewGuid());
        var talk = Domain.TalkManagement.Talk.Create(
            talkId,
            new TalkTitle(command.Title),
            new Abstract(command.Abstract),
            command.Tags.Select(t => new TalkTag(t)),
            speakerId
        );

        await talkRepository.Save(talk);

        return talkId.Value.Value;
    }
}
