using ConferenceExample.Conference.Domain.Repositories;
using ConferenceExample.Conference.Domain.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.GetConferenceSessions;

public class GetConferenceSessionsQueryHandler(IConferenceRepository conferenceRepository)
    : IGetConferenceSessionsQueryHandler
{
    public async Task<IReadOnlyList<GetConferenceSessionDto>> Handle(
        GetConferenceSessionsQuery query
    )
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(query.ConferenceId))
        );

        return conference
            .Talks.Select(talk => new GetConferenceSessionDto(
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
