using ConferenceExample.Talk.Domain.TalkManagement;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Talk.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTalkPersistence(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<ITalkRepository, TalkRepository>();

        return services;
    }
}
