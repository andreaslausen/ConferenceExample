using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Application.DeleteTalk;

public class DeleteTalkCommandHandler(
    ITalkRepository talkRepository,
    ICurrentSpeakerProvider currentSpeakerProvider
) : IDeleteTalkCommandHandler
{
    public async Task Handle(DeleteTalkCommand command)
    {
        var talk = await talkRepository.GetById(new TalkId(new GuidV7(command.TalkId)));

        var currentSpeakerId = await currentSpeakerProvider.GetCurrentSpeakerId();

        if (talk.SpeakerId != currentSpeakerId)
        {
            throw new UnauthorizedAccessException(
                $"Speaker {currentSpeakerId.Value} is not authorized to delete talk {talk.Id.Value}."
            );
        }

        talk.Delete();

        await talkRepository.Save(talk);
    }
}
