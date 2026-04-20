namespace ConferenceExample.Talk.Application.GetMyProfile;

public interface IGetMyProfileQueryHandler
{
    Task<GetMyProfileDto?> Handle(GetMyProfileQuery query);
}
