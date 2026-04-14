using ConferenceExample.Authentication;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Conference.Application.GetConferenceTalks;

public class GetConferenceTalksQueryHandler(
    ITalkRepository talkRepository,
    IConferenceRepository conferenceRepository,
    ICurrentUserService currentUserService
) : IGetConferenceTalksQueryHandler
{
    public async Task<IReadOnlyList<GetConferenceTalksDto>> Handle(GetConferenceTalksQuery query)
    {
        // Get the conference to check ownership
        var conference = await conferenceRepository.GetById(
            new Conference.Domain.ConferenceManagement.ConferenceId(
                new Conference.Domain.SharedKernel.ValueObjects.Ids.GuidV7(query.ConferenceId)
            )
        );

        // Check ownership: Only the organizer who created the conference can view submitted talks
        var currentUserId = currentUserService.GetCurrentUserId();
        var currentOrganizerId = new OrganizerId(
            new Conference.Domain.SharedKernel.ValueObjects.Ids.GuidV7(currentUserId.Value.Value)
        );

        if (conference.OrganizerId.Value != currentOrganizerId.Value)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId.Value} is not authorized to view talks for conference {conference.Id.Value}. Only the organizer who created the conference can view submitted talks."
            );
        }

        // Get all talks for this conference from the Talk bounded context
        var talks = await talkRepository.GetTalks(
            new Talk.Domain.TalkManagement.ConferenceId(
                new Talk.Domain.SharedKernel.ValueObjects.Ids.GuidV7(query.ConferenceId)
            )
        );

        return talks
            .Select(talk => new GetConferenceTalksDto(
                talk.Id.Value.Value,
                talk.Title.Title,
                talk.Abstract.Content,
                talk.SpeakerId.Value.Value,
                talk.Status.ToString(),
                talk.Tags.Select(t => t.Tag).ToList(),
                talk.TalkTypeId.Value.Value
            ))
            .ToList();
    }
}
