using ConferenceExample.Talk.Application.CreateTalk;
using ConferenceExample.Talk.Application.DeleteTalk;
using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Application.GetTalkById;
using ConferenceExample.Talk.Application.GetTalkSubmissions;
using ConferenceExample.Talk.Application.SubmitTalkToConference;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Talk.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTalkApplication(this IServiceCollection services)
    {
        // Command Handlers
        services.AddScoped<ICreateTalkCommandHandler, CreateTalkCommandHandler>();
        services.AddScoped<IEditTalkCommandHandler, EditTalkCommandHandler>();
        services.AddScoped<IDeleteTalkCommandHandler, DeleteTalkCommandHandler>();
        services.AddScoped<
            ISubmitTalkToConferenceCommandHandler,
            SubmitTalkToConferenceCommandHandler
        >();

        // Query Handlers
        services.AddScoped<IGetMyTalksQueryHandler, GetMyTalksQueryHandler>();
        services.AddScoped<IGetTalkByIdQueryHandler, GetTalkByIdQueryHandler>();
        services.AddScoped<IGetTalkSubmissionsQueryHandler, GetTalkSubmissionsQueryHandler>();

        // Services
        services.AddScoped<ICurrentSpeakerProvider, CurrentSpeakerProvider>();
        services.AddScoped<ITalkService, TalkService>();

        return services;
    }
}
