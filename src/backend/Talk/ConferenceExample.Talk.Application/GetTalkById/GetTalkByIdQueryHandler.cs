using ConferenceExample.Talk.Domain.ConferenceManagement;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Application.GetTalkById;

public class GetTalkByIdQueryHandler(
    ITalkReadModelRepository talkReadModelRepository,
    IConferenceOrganizerReadModelRepository conferenceOrganizerReadModelRepository,
    ICurrentUserService currentUserService
) : IGetTalkByIdQueryHandler
{
    public async Task<GetTalkByIdDto?> Handle(GetTalkByIdQuery query)
    {
        var talkId = new TalkId(new GuidV7(query.TalkId));
        var talk = await talkReadModelRepository.GetById(talkId);

        if (talk is null)
            return null;

        if (talk.Status != TalkStatus.Accepted.ToString() && !await IsAuthorized(talk))
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

    private async Task<bool> IsAuthorized(TalkReadModel talk)
    {
        GuidV7 currentUserId;
        try
        {
            currentUserId = new GuidV7(currentUserService.GetCurrentUserId());
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }

        if (new GuidV7(talk.SpeakerId) == currentUserId)
            return true;

        var organizer = await conferenceOrganizerReadModelRepository.GetByConferenceId(
            talk.ConferenceId
        );
        return organizer is not null && new GuidV7(organizer.OrganizerId) == currentUserId;
    }
}
