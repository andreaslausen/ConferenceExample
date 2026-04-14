using ConferenceExample.Authentication;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Application.SubmitTalk;

public class SubmitTalkCommandHandler(
    ITalkRepository talkRepository,
    ICurrentUserService currentUserService
) : ISubmitTalkCommandHandler
{
    public async Task Handle(SubmitTalkCommand command)
    {
        // Get the current authenticated user's ID
        // This ensures that a speaker can only submit talks for themselves
        var currentUserId = currentUserService.GetCurrentUserId();

        // Convert Authentication.GuidV7 to Talk.Domain.GuidV7
        var speakerId = new SpeakerId(new GuidV7(currentUserId.Value.Value));

        var talk = Domain.TalkManagement.Talk.Submit(
            new TalkId(GuidV7.NewGuid()),
            new TalkTitle(command.Title),
            speakerId,
            command.Tags.Select(t => new TalkTag(t)),
            new TalkTypeId(command.TalkTypeId),
            new Abstract(command.Abstract),
            new ConferenceId(command.ConferenceId)
        );

        await talkRepository.Save(talk);
    }
}
