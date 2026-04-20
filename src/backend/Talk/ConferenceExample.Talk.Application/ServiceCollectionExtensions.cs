using ConferenceExample.Talk.Application.CreateSpeakerProfile;
using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Application.GetMyProfile;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Application.GetSpeakerById;
using ConferenceExample.Talk.Application.SubmitTalk;
using ConferenceExample.Talk.Application.UpdateSpeakerProfile;
using ConferenceExample.Talk.Domain.TalkManagement;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Talk.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTalkApplication(this IServiceCollection services)
    {
        // Talk Command Handlers
        services.AddScoped<ISubmitTalkCommandHandler, SubmitTalkCommandHandler>();
        services.AddScoped<IEditTalkCommandHandler, EditTalkCommandHandler>();

        // Talk Query Handlers
        services.AddScoped<IGetMyTalksQueryHandler, GetMyTalksQueryHandler>();

        // Speaker Command Handlers
        services.AddScoped<
            ICreateSpeakerProfileCommandHandler,
            CreateSpeakerProfileCommandHandler
        >();
        services.AddScoped<
            IUpdateSpeakerProfileCommandHandler,
            UpdateSpeakerProfileCommandHandler
        >();

        // Speaker Query Handlers
        services.AddScoped<IGetMyProfileQueryHandler, GetMyProfileQueryHandler>();
        services.AddScoped<IGetSpeakerByIdQueryHandler, GetSpeakerByIdQueryHandler>();

        // Services
        services.AddScoped<ITalkService, TalkService>();
        services.AddScoped<ISpeakerService, SpeakerService>();

        return services;
    }
}
