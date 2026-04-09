using ConferenceExample.Talk.Application.Dtos;

namespace ConferenceExample.Talk.Application;

public interface ITalkService
{
    Task SubmitTalk(SubmitTalkDto submitTalkDto);
}
