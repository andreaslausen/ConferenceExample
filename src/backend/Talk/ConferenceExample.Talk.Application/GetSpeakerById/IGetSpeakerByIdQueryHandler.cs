namespace ConferenceExample.Talk.Application.GetSpeakerById;

public interface IGetSpeakerByIdQueryHandler
{
    Task<GetSpeakerByIdDto?> Handle(GetSpeakerByIdQuery query);
}
