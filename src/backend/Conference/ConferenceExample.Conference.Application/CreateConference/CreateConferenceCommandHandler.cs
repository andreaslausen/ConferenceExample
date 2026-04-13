using ConferenceExample.Conference.Domain.Repositories;
using ConferenceExample.Conference.Domain.ValueObjects;
using ConferenceExample.Conference.Domain.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.CreateConference;

public class CreateConferenceCommandHandler(IConferenceRepository conferenceRepository)
    : ICreateConferenceCommandHandler
{
    public async Task<ConferenceCreatedDto> Handle(CreateConferenceCommand command)
    {
        var conference = Domain.Conference.Create(
            new ConferenceId(GuidV7.NewGuid()),
            new Text(command.Name),
            new Time(command.Start, command.End),
            new Location(
                new Text(command.LocationName),
                new Address(
                    command.Street,
                    command.City,
                    command.State,
                    command.PostalCode,
                    command.Country
                )
            )
        );

        await conferenceRepository.Save(conference);

        return new ConferenceCreatedDto(
            conference.Id.Value,
            conference.Name.Value,
            conference.ConferenceTime.Start,
            conference.ConferenceTime.End,
            conference.Location.Name.Value
        );
    }
}
