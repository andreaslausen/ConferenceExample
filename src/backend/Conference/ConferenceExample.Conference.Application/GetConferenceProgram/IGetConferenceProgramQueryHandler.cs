namespace ConferenceExample.Conference.Application.GetConferenceProgram;

public interface IGetConferenceProgramQueryHandler
{
    Task<IReadOnlyList<GetConferenceProgramDto>> Handle(GetConferenceProgramQuery query);
}
