using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Application.SubmitTalk;

namespace ConferenceExample.Talk.Application;

public interface ITalkService
{
    Task SubmitTalk(SubmitTalkDto submitTalkDto);
    Task<IReadOnlyList<GetMyTalksDto>> GetMyTalks();
    Task EditTalk(Guid talkId, EditTalkDto editTalkDto);
}
