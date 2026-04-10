using ConferenceExample.Conference.Domain.Repositories;
using ConferenceExample.Conference.Domain.ValueObjects;
using ConferenceExample.Conference.Domain.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.Commands;

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
