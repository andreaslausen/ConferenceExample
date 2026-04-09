using ConferenceExample.Talk.Application.Commands;
using ConferenceExample.Talk.Application.Dtos;

namespace ConferenceExample.Talk.Application;

public class TalkService(ISubmitTalkCommandHandler submitTalkCommandHandler) : ITalkService
{
    public async Task SubmitTalk(SubmitTalkDto submitTalkDto)
    {
        var command = new SubmitTalkCommand(
            submitTalkDto.Title,
            submitTalkDto.Abstract,
            submitTalkDto.ConferenceId,
            submitTalkDto.SpeakerId,
            submitTalkDto.Tags,
            submitTalkDto.TalkTypeId
        );

        await submitTalkCommandHandler.Handle(command);
    }
}
