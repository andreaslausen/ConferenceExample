using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.TalkManagement;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Conference.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConferencePersistence(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IConferenceRepository, ConferenceRepository>();
        services.AddScoped<ITalkRepository, TalkRepository>();

        return services;
    }
}
