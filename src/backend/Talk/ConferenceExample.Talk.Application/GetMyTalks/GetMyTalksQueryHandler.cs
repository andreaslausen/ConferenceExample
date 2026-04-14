using ConferenceExample.Authentication;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Application.GetMyTalks;

public class GetMyTalksQueryHandler(
    ITalkRepository talkRepository,
    ICurrentUserService currentUserService
) : IGetMyTalksQueryHandler
{
    public async Task<IReadOnlyList<GetMyTalksDto>> Handle(GetMyTalksQuery query)
    {
        // Get the current authenticated speaker's ID
        var currentUserId = currentUserService.GetCurrentUserId();
        var speakerId = new SpeakerId(new GuidV7(currentUserId.Value.Value));

        var talks = await talkRepository.GetTalksBySpeaker(speakerId);

        return talks
            .Select(talk => new GetMyTalksDto(
                talk.Id.Value,
                talk.Title.Title,
                talk.Abstract.Content,
                talk.ConferenceId.Value,
                talk.Status.ToString(),
                talk.Tags.Select(t => t.Tag).ToList()
            ))
            .ToList();
    }
}
