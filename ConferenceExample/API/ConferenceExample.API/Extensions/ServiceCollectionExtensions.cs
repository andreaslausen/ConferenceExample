using ConferenceExample.API.Infrastructure;
using ConferenceExample.Conference.Application;
using ConferenceExample.Persistence;

namespace ConferenceExample.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        var counterIdValueGeneratorStrategy = new CounterIdValueGeneratorStrategy();
        services.AddSingleton<IIdValueGeneratorStrategy>(counterIdValueGeneratorStrategy);
        services.AddSingleton<Session.Application.IIdValueGeneratorStrategy>(counterIdValueGeneratorStrategy);
        services.AddSingleton<IDatabaseContext, DatabaseContext>();
        return services;
    }
}