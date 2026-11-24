using ConferenceExample.Speaker.Application.CreateSpeakerProfile;
using ConferenceExample.Speaker.Application.GetMyProfile;
using ConferenceExample.Speaker.Application.GetSpeakerById;
using ConferenceExample.Speaker.Application.UpdateSpeakerProfile;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Speaker.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSpeakerApplication(this IServiceCollection services)
    {
        // Command Handlers
        services.AddScoped<
            ICreateSpeakerProfileCommandHandler,
            CreateSpeakerProfileCommandHandler
        >();
        services.AddScoped<
            IUpdateSpeakerProfileCommandHandler,
            UpdateSpeakerProfileCommandHandler
        >();

        // Query Handlers
        services.AddScoped<IGetMyProfileQueryHandler, GetMyProfileQueryHandler>();
        services.AddScoped<IGetSpeakerByIdQueryHandler, GetSpeakerByIdQueryHandler>();

        // Services
        services.AddScoped<ISpeakerService, SpeakerService>();

        return services;
    }
}
