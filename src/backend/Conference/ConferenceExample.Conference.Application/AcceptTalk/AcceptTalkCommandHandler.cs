using ConferenceExample.Authentication;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;

namespace ConferenceExample.Conference.Application.AcceptTalk;

public class AcceptTalkCommandHandler(
    IConferenceRepository conferenceRepository,
    ICurrentUserService currentUserService
) : IAcceptTalkCommandHandler
{
    public async Task Handle(AcceptTalkCommand command)
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(command.ConferenceId))
        );

        // Check ownership: Only the organizer who created the conference can accept talks
        var currentUserId = currentUserService.GetCurrentUserId();
        var currentOrganizerId = new OrganizerId(new GuidV7(currentUserId.Value.Value));

        if (conference.OrganizerId.Value != currentOrganizerId.Value)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId.Value} is not authorized to accept talks for conference {conference.Id.Value}. Only the organizer who created the conference can accept talks."
            );
        }

        conference.AcceptTalk(new TalkId(new GuidV7(command.TalkId)));
        await conferenceRepository.Save(conference);
    }
}
