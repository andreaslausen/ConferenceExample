using ConferenceExample.Authentication;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;

namespace ConferenceExample.Conference.Application.RejectTalk;

public class RejectTalkCommandHandler(
    IConferenceRepository conferenceRepository,
    ICurrentUserService currentUserService
) : IRejectTalkCommandHandler
{
    public async Task Handle(RejectTalkCommand command)
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(command.ConferenceId))
        );

        // Check ownership: Only the organizer who created the conference can reject talks
        var currentUserId = currentUserService.GetCurrentUserId();
        var currentOrganizerId = new OrganizerId(new GuidV7(currentUserId.Value.Value));

        if (conference.OrganizerId.Value != currentOrganizerId.Value)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId.Value} is not authorized to reject talks for conference {conference.Id.Value}. Only the organizer who created the conference can reject talks."
            );
        }

        conference.RejectTalk(new TalkId(new GuidV7(command.TalkId)));
        await conferenceRepository.Save(conference);
    }
}
