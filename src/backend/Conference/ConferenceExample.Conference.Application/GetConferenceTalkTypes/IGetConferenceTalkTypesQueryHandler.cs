namespace ConferenceExample.Conference.Application.GetConferenceTalkTypes;

public interface IGetConferenceTalkTypesQueryHandler
{
    Task<IReadOnlyList<GetConferenceTalkTypesDto>> Handle(GetConferenceTalkTypesQuery query);
}
