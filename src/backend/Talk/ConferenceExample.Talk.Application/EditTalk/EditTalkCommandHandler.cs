using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Application.EditTalk;

public class EditTalkCommandHandler(
    ITalkRepository talkRepository,
    ICurrentSpeakerProvider currentSpeakerProvider
) : IEditTalkCommandHandler
{
    public async Task Handle(EditTalkCommand command)
    {
        var talk = await talkRepository.GetById(new TalkId(new GuidV7(command.TalkId)));

        // Only the speaker who owns the talk may edit it. Submissions already registered by a
        // conference keep the content they were submitted with — see Talk.SubmitToConference.
        var currentSpeakerId = await currentSpeakerProvider.GetCurrentSpeakerId();

        if (talk.SpeakerId != currentSpeakerId)
        {
            throw new UnauthorizedAccessException(
                $"Speaker {currentSpeakerId.Value} is not authorized to edit talk {talk.Id.Value}."
            );
        }

        talk.EditTitle(new TalkTitle(command.Title));
        talk.EditAbstract(new Abstract(command.Abstract));

        // Update tags: remove all existing tags and add new ones
        foreach (var existingTag in talk.Tags.ToList())
        {
            talk.RemoveTag(existingTag);
        }

        foreach (var newTag in command.Tags)
        {
            talk.AddTag(new TalkTag(newTag));
        }

        await talkRepository.Save(talk);
    }
}
