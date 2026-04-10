using ConferenceExample.Conference.Application.Dtos;
using ConferenceExample.Conference.Domain.Repositories;
using ConferenceExample.Conference.Domain.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.Queries;

public class GetConferenceSessionsQueryHandler(IConferenceRepository conferenceRepository)
    : IGetConferenceSessionsQueryHandler
{
    public async Task<IReadOnlyList<SessionDto>> Handle(GetConferenceSessionsQuery query)
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(query.ConferenceId))
        );

        return conference
            .Talks.Select(talk => new SessionDto(
                talk.Id.Value,
                talk.Status.ToString(),
                talk.Slot?.Start,
                talk.Slot?.End,
                talk.Room?.Id.Value.Value,
                talk.Room?.Name.Value
            ))
            .ToList();
    }
}
