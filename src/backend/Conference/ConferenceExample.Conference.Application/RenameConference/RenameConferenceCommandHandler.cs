using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.RenameConference;

public class RenameConferenceCommandHandler(IConferenceRepository conferenceRepository)
    : IRenameConferenceCommandHandler
{
    public async Task Handle(RenameConferenceCommand command)
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(command.Id))
        );
        conference.Rename(new Text(command.Name));
        await conferenceRepository.Save(conference);
    }
}
