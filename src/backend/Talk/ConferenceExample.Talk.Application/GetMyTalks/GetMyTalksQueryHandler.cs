using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Application.GetMyTalks;

public class GetMyTalksQueryHandler(
    ITalkReadModelRepository talkReadModelRepository,
    ICurrentSpeakerProvider currentSpeakerProvider
) : IGetMyTalksQueryHandler
{
    public async Task<(IReadOnlyList<GetMyTalksDto> Items, int TotalCount)> Handle(
        GetMyTalksQuery query
    )
    {
        var speakerId = await currentSpeakerProvider.GetCurrentSpeakerId();

        var (talks, totalCount) = await talkReadModelRepository.GetBySpeakerId(
            speakerId,
            query.PageRequest
        );

        var items = talks
            .Select(talk => new GetMyTalksDto(
                talk.Id,
                talk.Title,
                talk.Abstract,
                talk.Tags.ToList(),
                talk.SubmissionCount
            ))
            .ToList();

        return (items, totalCount);
    }
}
