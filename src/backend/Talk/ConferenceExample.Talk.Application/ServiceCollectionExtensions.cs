using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Application.SubmitTalk;
using ConferenceExample.Talk.Domain.TalkManagement;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Talk.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTalkApplication(this IServiceCollection services)
    {
        // Command Handlers
        services.AddScoped<ISubmitTalkCommandHandler, SubmitTalkCommandHandler>();
        services.AddScoped<IEditTalkCommandHandler, EditTalkCommandHandler>();

        // Query Handlers
        services.AddScoped<IGetMyTalksQueryHandler, GetMyTalksQueryHandler>();

        // Services
        services.AddScoped<ITalkService, TalkService>();

        return services;
    }
}
