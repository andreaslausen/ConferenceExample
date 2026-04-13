using ConferenceExample.Talk.Domain.Repositories;
using ConferenceExample.Talk.Domain.ValueObjects;
using ConferenceExample.Talk.Domain.ValueObjects.Ids;

namespace ConferenceExample.Talk.Application.SubmitTalk;

public class SubmitTalkCommandHandler(ITalkRepository talkRepository) : ISubmitTalkCommandHandler
{
    public async Task Handle(SubmitTalkCommand command)
    {
        var talk = Domain.Entities.Talk.Submit(
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
