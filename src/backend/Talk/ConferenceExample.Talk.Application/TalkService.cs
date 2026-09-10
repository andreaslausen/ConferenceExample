using ConferenceExample.Talk.Application.CreateTalk;
using ConferenceExample.Talk.Application.DeleteTalk;
using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Application.GetTalkById;
using ConferenceExample.Talk.Application.GetTalkSubmissions;
using ConferenceExample.Talk.Application.SubmitTalkToConference;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects;

namespace ConferenceExample.Talk.Application;

public class TalkService(
    ICreateTalkCommandHandler createTalkCommandHandler,
    IEditTalkCommandHandler editTalkCommandHandler,
    IDeleteTalkCommandHandler deleteTalkCommandHandler,
    ISubmitTalkToConferenceCommandHandler submitTalkToConferenceCommandHandler,
    IGetMyTalksQueryHandler getMyTalksQueryHandler,
    IGetTalkByIdQueryHandler getTalkByIdQueryHandler,
    IGetTalkSubmissionsQueryHandler getTalkSubmissionsQueryHandler
) : ITalkService
{
    public async Task<Guid> CreateTalk(CreateTalkDto createTalkDto)
    {
        var command = new CreateTalkCommand(
            createTalkDto.Title,
            createTalkDto.Abstract,
            createTalkDto.Tags
        );

        return await createTalkCommandHandler.Handle(command);
    }

    public async Task<(IReadOnlyList<GetMyTalksDto> Items, int TotalCount)> GetMyTalks(
        int page,
        int pageSize
    )
    {
        var query = new GetMyTalksQuery(new PageRequest(page, pageSize));
        return await getMyTalksQueryHandler.Handle(query);
    }

    public async Task<GetTalkByIdDto?> GetTalkById(Guid talkId)
    {
        var query = new GetTalkByIdQuery(talkId);
        return await getTalkByIdQueryHandler.Handle(query);
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

    public async Task DeleteTalk(Guid talkId)
    {
        await deleteTalkCommandHandler.Handle(new DeleteTalkCommand(talkId));
    }

    public async Task SubmitTalkToConference(Guid talkId, SubmitTalkToConferenceDto submitTalkDto)
    {
        var command = new SubmitTalkToConferenceCommand(
            talkId,
            submitTalkDto.ConferenceId,
            submitTalkDto.TalkTypeId
        );

        await submitTalkToConferenceCommandHandler.Handle(command);
    }

    public async Task<IReadOnlyList<GetTalkSubmissionsDto>?> GetTalkSubmissions(Guid talkId)
    {
        return await getTalkSubmissionsQueryHandler.Handle(new GetTalkSubmissionsQuery(talkId));
    }
}
