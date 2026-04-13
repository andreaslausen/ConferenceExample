using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Application.SubmitTalk;

public class SubmitTalkCommandHandler(ITalkRepository talkRepository) : ISubmitTalkCommandHandler
{
    public async Task Handle(SubmitTalkCommand command)
    {
        var talk = Domain.TalkManagement.Talk.Submit(
            new TalkId(GuidV7.NewGuid()),
            new TalkTitle(command.Title),
            new SpeakerId(command.SpeakerId),
            command.Tags.Select(t => new TalkTag(t)),
            new TalkTypeId(command.TalkTypeId),
            new Abstract(command.Abstract),
            new ConferenceId(command.ConferenceId)
        );

        await talkRepository.Save(talk);
    }
}
