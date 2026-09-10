namespace ConferenceExample.Speaker.Application.GetSpeakerById;

public interface IGetSpeakerByIdQueryHandler
{
    Task<GetSpeakerByIdDto?> Handle(GetSpeakerByIdQuery query);
}
