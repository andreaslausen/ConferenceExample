using ConferenceExample.Conference.Application;

namespace ConferenceExample.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        var counterIdValueGeneratorStrategy = new CounterIdValueGeneratorStrategy();
        services.AddSingleton<IIdValueGeneratorStrategy>(counterIdValueGeneratorStrategy);
        services.AddSingleton<Session.Application.IIdValueGeneratorStrategy>(counterIdValueGeneratorStrategy);
        return services;
    }
}