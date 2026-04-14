using ConferenceExample.Authentication;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Application.EditTalk;

public class EditTalkCommandHandler(
    ITalkRepository talkRepository,
    ICurrentUserService currentUserService
) : IEditTalkCommandHandler
{
    public async Task Handle(EditTalkCommand command)
    {
        // Get the talk
        var talk = await talkRepository.GetById(new TalkId(new GuidV7(command.TalkId)));

        // Check ownership: Only the speaker who created the talk can edit it
        var currentUserId = currentUserService.GetCurrentUserId();
        var currentSpeakerId = new SpeakerId(new GuidV7(currentUserId.Value.Value));

        if (talk.SpeakerId != currentSpeakerId)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId.Value} is not authorized to edit talk {talk.Id.Value}. Only the speaker who created the talk can edit it."
            );
        }

        // Edit the talk
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
