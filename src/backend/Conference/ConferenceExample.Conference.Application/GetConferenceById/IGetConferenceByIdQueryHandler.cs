namespace ConferenceExample.Conference.Application.GetConferenceById;

public interface IGetConferenceByIdQueryHandler
{
    Task<GetConferenceByIdDto> Handle(GetConferenceByIdQuery query);
}
