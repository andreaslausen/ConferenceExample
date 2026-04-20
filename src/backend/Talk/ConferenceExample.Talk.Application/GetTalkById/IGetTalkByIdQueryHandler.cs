namespace ConferenceExample.Talk.Application.GetTalkById;

public interface IGetTalkByIdQueryHandler
{
    Task<GetTalkByIdDto?> Handle(GetTalkByIdQuery query);
}
