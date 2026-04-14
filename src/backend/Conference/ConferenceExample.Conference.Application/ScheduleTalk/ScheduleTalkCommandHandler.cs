using ConferenceExample.Authentication;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;

namespace ConferenceExample.Conference.Application.ScheduleTalk;

public class ScheduleTalkCommandHandler(
    IConferenceRepository conferenceRepository,
    ICurrentUserService currentUserService
) : IScheduleTalkCommandHandler
{
    public async Task Handle(ScheduleTalkCommand command)
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(command.ConferenceId))
        );

        // Check ownership: Only the organizer who created the conference can schedule talks
        var currentUserId = currentUserService.GetCurrentUserId();
        var currentOrganizerId = new OrganizerId(new GuidV7(currentUserId.Value.Value));

        if (conference.OrganizerId.Value != currentOrganizerId.Value)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId.Value} is not authorized to schedule talks for conference {conference.Id.Value}. Only the organizer who created the conference can schedule talks."
            );
        }

        conference.ScheduleTalk(
            new TalkId(new GuidV7(command.TalkId)),
            new Time(command.Start, command.End)
        );
        await conferenceRepository.Save(conference);
    }
}
