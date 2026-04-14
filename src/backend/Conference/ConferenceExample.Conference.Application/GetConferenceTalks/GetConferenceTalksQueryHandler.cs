using ConferenceExample.Authentication;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;

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
            new ConferenceId(new GuidV7(query.ConferenceId))
        );

        // Check ownership: Only the organizer who created the conference can view submitted talks
        var currentUserId = currentUserService.GetCurrentUserId();
        var currentOrganizerId = new OrganizerId(new GuidV7(currentUserId.Value.Value));

        if (conference.OrganizerId.Value != currentOrganizerId.Value)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId.Value} is not authorized to view talks for conference {conference.Id.Value}. Only the organizer who created the conference can view submitted talks."
            );
        }

        // Get all talks for this conference from our local read model
        var talks = await talkRepository.GetTalksByConferenceId(conference.Id);

        return talks
            .Select(talk => new GetConferenceTalksDto(
                talk.Id.Value.Value,
                talk.Title?.Value ?? "",
                talk.Abstract?.Value ?? "",
                talk.SpeakerId?.Value ?? Guid.Empty,
                talk.Status.ToString(),
                talk.Tags.ToList(),
                talk.TalkTypeId?.Value ?? Guid.Empty
            ))
            .ToList();
    }
}
