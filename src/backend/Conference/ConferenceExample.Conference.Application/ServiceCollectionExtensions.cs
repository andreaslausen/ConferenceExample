using ConferenceExample.Conference.Application.AcceptTalk;
using ConferenceExample.Conference.Application.AddRoom;
using ConferenceExample.Conference.Application.AssignTalkToRoom;
using ConferenceExample.Conference.Application.ChangeConferenceStatus;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.DefineTalkType;
using ConferenceExample.Conference.Application.GetAllConferences;
using ConferenceExample.Conference.Application.GetConferenceById;
using ConferenceExample.Conference.Application.GetConferenceProgram;
using ConferenceExample.Conference.Application.GetConferenceRooms;
using ConferenceExample.Conference.Application.GetConferenceSchedule;
using ConferenceExample.Conference.Application.GetConferenceTalks;
using ConferenceExample.Conference.Application.GetConferenceTalkTypes;
using ConferenceExample.Conference.Application.GetMyConferences;
using ConferenceExample.Conference.Application.RejectTalk;
using ConferenceExample.Conference.Application.RemoveRoom;
using ConferenceExample.Conference.Application.RemoveTalkType;
using ConferenceExample.Conference.Application.RenameConference;
using ConferenceExample.Conference.Application.ScheduleTalk;
using ConferenceExample.Conference.Application.UpdateConferenceDetails;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Conference.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConferenceApplication(this IServiceCollection services)
    {
        // Command Handlers
        services.AddScoped<ICreateConferenceCommandHandler, CreateConferenceCommandHandler>();
        services.AddScoped<IRenameConferenceCommandHandler, RenameConferenceCommandHandler>();
        services.AddScoped<
            IUpdateConferenceDetailsCommandHandler,
            UpdateConferenceDetailsCommandHandler
        >();
        services.AddScoped<
            IChangeConferenceStatusCommandHandler,
            ChangeConferenceStatusCommandHandler
        >();
        services.AddScoped<IAcceptTalkCommandHandler, AcceptTalkCommandHandler>();
        services.AddScoped<IRejectTalkCommandHandler, RejectTalkCommandHandler>();
        services.AddScoped<IScheduleTalkCommandHandler, ScheduleTalkCommandHandler>();
        services.AddScoped<IAssignTalkToRoomCommandHandler, AssignTalkToRoomCommandHandler>();
        services.AddScoped<IDefineTalkTypeCommandHandler, DefineTalkTypeCommandHandler>();
        services.AddScoped<IRemoveTalkTypeCommandHandler, RemoveTalkTypeCommandHandler>();

        services.AddScoped<IAddRoomCommandHandler, AddRoomCommandHandler>();
        services.AddScoped<IRemoveRoomCommandHandler, RemoveRoomCommandHandler>();

        // Query Handlers
        services.AddScoped<IGetAllConferencesQueryHandler, GetAllConferencesQueryHandler>();
        services.AddScoped<IGetMyConferencesQueryHandler, GetMyConferencesQueryHandler>();
        services.AddScoped<IGetConferenceByIdQueryHandler, GetConferenceByIdQueryHandler>();
        services.AddScoped<IGetConferenceScheduleQueryHandler, GetConferenceScheduleQueryHandler>();
        services.AddScoped<IGetConferenceTalksQueryHandler, GetConferenceTalksQueryHandler>();
        services.AddScoped<
            IGetConferenceTalkTypesQueryHandler,
            GetConferenceTalkTypesQueryHandler
        >();
        services.AddScoped<IGetConferenceProgramQueryHandler, GetConferenceProgramQueryHandler>();
        services.AddScoped<IGetConferenceRoomsQueryHandler, GetConferenceRoomsQueryHandler>();

        // Services
        services.AddScoped<IConferenceService, ConferenceService>();

        return services;
    }
}
