namespace ConferenceExample.Talk.Application.GetMyTalks;

public interface IGetMyTalksQueryHandler
{
    Task<(IReadOnlyList<GetMyTalksDto> Items, int TotalCount)> Handle(GetMyTalksQuery query);
}
