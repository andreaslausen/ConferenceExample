using ConferenceExample.Authentication;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;

namespace ConferenceExample.Conference.Application.GetConferenceTalks;

public class GetConferenceTalksQueryHandler(
    IConferenceTalkReadModelRepository talkReadModelRepository,
    IConferenceRepository conferenceRepository,
    ICurrentUserService currentUserService
) : IGetConferenceTalksQueryHandler
{
    public async Task<IReadOnlyList<GetConferenceTalksDto>> Handle(GetConferenceTalksQuery query)
    {
        var conferenceId = new ConferenceId(new GuidV7(query.ConferenceId));
        var conference = await conferenceRepository.GetById(conferenceId);

        var currentUserId = currentUserService.GetCurrentUserId();
        var currentOrganizerId = new OrganizerId(new GuidV7(currentUserId.Value.Value));

        if (conference.OrganizerId.Value != currentOrganizerId.Value)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId.Value} is not authorized to view talks for conference {conference.Id.Value}. Only the organizer who created the conference can view submitted talks."
            );
        }

        var talks = await talkReadModelRepository.GetByConferenceId(conference.Id);

        return talks
            .Select(talk => new GetConferenceTalksDto(
                talk.Id,
                talk.Title,
                talk.Abstract,
                talk.SpeakerId,
                talk.Status,
                talk.Tags.ToList(),
                talk.TalkTypeId
            ))
            .ToList();
    }
}
