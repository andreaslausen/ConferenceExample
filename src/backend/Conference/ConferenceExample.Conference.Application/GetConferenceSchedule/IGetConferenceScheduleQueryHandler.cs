namespace ConferenceExample.Conference.Application.GetConferenceSchedule;

public interface IGetConferenceScheduleQueryHandler
{
    Task<IReadOnlyList<GetConferenceScheduleDto>> Handle(GetConferenceScheduleQuery query);
}
