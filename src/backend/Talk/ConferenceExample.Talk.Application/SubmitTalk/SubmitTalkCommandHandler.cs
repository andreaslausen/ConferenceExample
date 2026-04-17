using ConferenceExample.Authentication;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Application.SubmitTalk;

public class SubmitTalkCommandHandler(
    ITalkRepository talkRepository,
    ICurrentUserService currentUserService,
    IConferenceInfoRepository conferenceInfoRepository
) : ISubmitTalkCommandHandler
{
    public async Task Handle(SubmitTalkCommand command)
    {
        // Validate that the conference exists and is in CallForSpeakers status
        var conferenceId = new ConferenceId(new GuidV7(command.ConferenceId));
        var conferenceInfo = await conferenceInfoRepository.GetById(conferenceId);

        if (conferenceInfo is null)
        {
            throw new InvalidOperationException(
                $"Conference with id {command.ConferenceId} does not exist."
            );
        }

        if (conferenceInfo.Status != "CallForSpeakers")
        {
            throw new InvalidOperationException(
                $"Talks can only be submitted when the conference is in CallForSpeakers status. Current status: {conferenceInfo.Status}"
            );
        }

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
