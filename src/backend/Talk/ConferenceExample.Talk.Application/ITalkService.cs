using ConferenceExample.Talk.Application.CreateTalk;
using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Application.GetTalkById;
using ConferenceExample.Talk.Application.GetTalkSubmissions;
using ConferenceExample.Talk.Application.SubmitTalkToConference;

namespace ConferenceExample.Talk.Application;

public interface ITalkService
{
    Task<Guid> CreateTalk(CreateTalkDto createTalkDto);
    Task<(IReadOnlyList<GetMyTalksDto> Items, int TotalCount)> GetMyTalks(int page, int pageSize);
    Task<GetTalkByIdDto?> GetTalkById(Guid talkId);
    Task EditTalk(Guid talkId, EditTalkDto editTalkDto);
    Task DeleteTalk(Guid talkId);
    Task SubmitTalkToConference(Guid talkId, SubmitTalkToConferenceDto submitTalkDto);
    Task<IReadOnlyList<GetTalkSubmissionsDto>?> GetTalkSubmissions(Guid talkId);
}
