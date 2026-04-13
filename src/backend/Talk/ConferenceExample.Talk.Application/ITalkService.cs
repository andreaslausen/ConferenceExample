using ConferenceExample.Talk.Application.SubmitTalk;

namespace ConferenceExample.Talk.Application;

public interface ITalkService
{
    Task SubmitTalk(SubmitTalkDto submitTalkDto);
}
