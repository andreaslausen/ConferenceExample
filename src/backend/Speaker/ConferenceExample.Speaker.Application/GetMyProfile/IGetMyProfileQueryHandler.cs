namespace ConferenceExample.Speaker.Application.GetMyProfile;

public interface IGetMyProfileQueryHandler
{
    Task<GetMyProfileDto?> Handle(GetMyProfileQuery query);
}
