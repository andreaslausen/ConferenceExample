namespace ConferenceExample.Talk.Application.GetTalkSubmissions;

public interface IGetTalkSubmissionsQueryHandler
{
    Task<IReadOnlyList<GetTalkSubmissionsDto>?> Handle(GetTalkSubmissionsQuery query);
}
