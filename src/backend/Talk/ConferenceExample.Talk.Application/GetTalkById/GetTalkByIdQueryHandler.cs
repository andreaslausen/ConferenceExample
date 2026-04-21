using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Application.GetTalkById;

public class GetTalkByIdQueryHandler(ITalkReadModelRepository talkReadModelRepository)
    : IGetTalkByIdQueryHandler
{
    public async Task<GetTalkByIdDto?> Handle(GetTalkByIdQuery query)
    {
        var talkId = new TalkId(new GuidV7(query.TalkId));
        var talk = await talkReadModelRepository.GetById(talkId);

        if (talk is null)
            return null;

        return new GetTalkByIdDto(
            talk.Id,
            talk.Title,
            talk.Abstract,
            talk.ConferenceId,
            talk.Status,
            talk.Tags.ToList(),
            talk.SpeakerId,
            talk.SpeakerName
        );
    }
}
