using ConferenceExample.Authentication;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.ChangeConferenceStatus;

public class ChangeConferenceStatusCommandHandler(
    IConferenceRepository conferenceRepository,
    ICurrentUserService currentUserService
) : IChangeConferenceStatusCommandHandler
{
    public async Task Handle(ChangeConferenceStatusCommand command)
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(command.ConferenceId))
        );

        // Check ownership: Only the organizer who created the conference can change its status
        var currentUserId = currentUserService.GetCurrentUserId();
        var currentOrganizerId = new OrganizerId(new GuidV7(currentUserId.Value.Value));

        if (conference.OrganizerId.Value != currentOrganizerId.Value)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId.Value} is not authorized to change the status of conference {conference.Id.Value}. Only the organizer who created the conference can change its status."
            );
        }

        conference.ChangeStatus(command.NewStatus);
        await conferenceRepository.Save(conference);
    }
}
