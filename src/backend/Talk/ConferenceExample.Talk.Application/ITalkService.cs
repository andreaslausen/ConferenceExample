using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Application.GetTalkById;
using ConferenceExample.Talk.Application.SubmitTalk;

namespace ConferenceExample.Talk.Application;

public interface ITalkService
{
    Task<Guid> SubmitTalk(SubmitTalkDto submitTalkDto);
    Task<(IReadOnlyList<GetMyTalksDto> Items, int TotalCount)> GetMyTalks(int page, int pageSize);
    Task<GetTalkByIdDto?> GetTalkById(Guid talkId);
    Task EditTalk(Guid talkId, EditTalkDto editTalkDto);
}
