using ConferenceExample.Conference.Application.AcceptTalk;
using ConferenceExample.Conference.Application.AssignTalkToRoom;
using ConferenceExample.Conference.Application.ChangeConferenceStatus;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.DefineTalkType;
using ConferenceExample.Conference.Application.GetAllConferences;
using ConferenceExample.Conference.Application.GetConferenceById;
using ConferenceExample.Conference.Application.GetConferenceSessions;
using ConferenceExample.Conference.Application.GetConferenceTalks;
using ConferenceExample.Conference.Application.GetConferenceTalkTypes;
using ConferenceExample.Conference.Application.RejectTalk;
using ConferenceExample.Conference.Application.RemoveTalkType;
using ConferenceExample.Conference.Application.RenameConference;
using ConferenceExample.Conference.Application.ScheduleTalk;
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
            IChangeConferenceStatusCommandHandler,
            ChangeConferenceStatusCommandHandler
        >();
        services.AddScoped<IAcceptTalkCommandHandler, AcceptTalkCommandHandler>();
        services.AddScoped<IRejectTalkCommandHandler, RejectTalkCommandHandler>();
        services.AddScoped<IScheduleTalkCommandHandler, ScheduleTalkCommandHandler>();
        services.AddScoped<IAssignTalkToRoomCommandHandler, AssignTalkToRoomCommandHandler>();
        services.AddScoped<IDefineTalkTypeCommandHandler, DefineTalkTypeCommandHandler>();
        services.AddScoped<IRemoveTalkTypeCommandHandler, RemoveTalkTypeCommandHandler>();

        // Query Handlers
        services.AddScoped<IGetAllConferencesQueryHandler, GetAllConferencesQueryHandler>();
        services.AddScoped<IGetConferenceByIdQueryHandler, GetConferenceByIdQueryHandler>();
        services.AddScoped<IGetConferenceSessionsQueryHandler, GetConferenceSessionsQueryHandler>();
        services.AddScoped<IGetConferenceTalksQueryHandler, GetConferenceTalksQueryHandler>();
        services.AddScoped<
            IGetConferenceTalkTypesQueryHandler,
            GetConferenceTalkTypesQueryHandler
        >();

        // Services
        services.AddScoped<IConferenceService, ConferenceService>();

        return services;
    }
}
