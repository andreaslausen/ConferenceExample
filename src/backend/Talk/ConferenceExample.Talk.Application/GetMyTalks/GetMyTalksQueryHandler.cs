using ConferenceExample.Authentication;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Application.GetMyTalks;

public class GetMyTalksQueryHandler(
    ITalkReadModelRepository talkReadModelRepository,
    ICurrentUserService currentUserService
) : IGetMyTalksQueryHandler
{
    public async Task<IReadOnlyList<GetMyTalksDto>> Handle(GetMyTalksQuery query)
    {
        var currentUserId = currentUserService.GetCurrentUserId();
        var speakerId = new SpeakerId(new GuidV7(currentUserId.Value.Value));

        var talks = await talkReadModelRepository.GetBySpeakerId(speakerId);

        return talks
            .Select(talk => new GetMyTalksDto(
                talk.Id,
                talk.Title,
                talk.Abstract,
                talk.ConferenceId,
                talk.Status,
                talk.Tags.ToList()
            ))
            .ToList();
    }
}
