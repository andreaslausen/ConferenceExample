namespace ConferenceExample.Talk.Application.GetMyTalks;

public interface IGetMyTalksQueryHandler
{
    Task<IReadOnlyList<GetMyTalksDto>> Handle(GetMyTalksQuery query);
}
