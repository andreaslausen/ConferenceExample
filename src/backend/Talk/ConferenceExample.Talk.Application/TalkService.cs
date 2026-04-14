using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Application.SubmitTalk;

namespace ConferenceExample.Talk.Application;

public class TalkService(
    ISubmitTalkCommandHandler submitTalkCommandHandler,
    IGetMyTalksQueryHandler getMyTalksQueryHandler,
    IEditTalkCommandHandler editTalkCommandHandler
) : ITalkService
{
    public async Task SubmitTalk(SubmitTalkDto submitTalkDto)
    {
        var command = new SubmitTalkCommand(
            submitTalkDto.Title,
            submitTalkDto.Abstract,
            submitTalkDto.ConferenceId,
            submitTalkDto.Tags,
            submitTalkDto.TalkTypeId
        );

        await submitTalkCommandHandler.Handle(command);
    }

    public async Task<IReadOnlyList<GetMyTalksDto>> GetMyTalks()
    {
        var query = new GetMyTalksQuery();
        return await getMyTalksQueryHandler.Handle(query);
    }

    public async Task EditTalk(Guid talkId, EditTalkDto editTalkDto)
    {
        var command = new EditTalkCommand(
            talkId,
            editTalkDto.Title,
            editTalkDto.Abstract,
            editTalkDto.Tags
        );

        await editTalkCommandHandler.Handle(command);
    }
}
