using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Application.SubmitTalk;
using ConferenceExample.Talk.Domain.TalkManagement;
using ConferenceExample.Talk.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Talk.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTalkContext(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<ITalkRepository, TalkRepository>();

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
