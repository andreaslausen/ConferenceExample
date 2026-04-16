using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.GetConferenceTalkTypes;

public class GetConferenceTalkTypesQueryHandler(IConferenceRepository conferenceRepository)
    : IGetConferenceTalkTypesQueryHandler
{
    public async Task<IReadOnlyList<GetConferenceTalkTypesDto>> Handle(
        GetConferenceTalkTypesQuery query
    )
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(query.ConferenceId))
        );

        return conference
            .TalkTypes.Select(tt => new GetConferenceTalkTypesDto(tt.Id.Value.Value, tt.Name.Value))
            .ToList();
    }
}
