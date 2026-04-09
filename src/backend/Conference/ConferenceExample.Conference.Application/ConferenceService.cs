using ConferenceExample.Conference.Application.Dtos;
using ConferenceExample.Conference.Domain;
using ConferenceExample.Conference.Domain.Repositories;
using ConferenceExample.Conference.Domain.ValueObjects;
using ConferenceExample.Conference.Domain.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application;

public class ConferenceService(IConferenceRepository conferenceRepository) : IConferenceService
{
    public async Task<ConferenceCreatedDto> CreateConference(CreateConferenceDto dto)
    {
        var conference = Domain.Conference.Create(
            new ConferenceId(GuidV7.NewGuid()),
            new Text(dto.Name),
            new Time(dto.Start, dto.End),
            new Location(
                new Text(dto.LocationName),
                new Address(dto.Street, dto.City, dto.State, dto.PostalCode, dto.Country)
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
