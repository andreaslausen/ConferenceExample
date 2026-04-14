using System.Text;
using System.Text.Json;
using ConferenceExample.Authentication;
using ConferenceExample.Conference.Application;
using ConferenceExample.Conference.Application.AcceptTalk;
using ConferenceExample.Conference.Application.AssignTalkToRoom;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.GetAllConferences;
using ConferenceExample.Conference.Application.GetConferenceById;
using ConferenceExample.Conference.Application.GetConferenceSessions;
using ConferenceExample.Conference.Application.GetConferenceTalks;
using ConferenceExample.Conference.Application.RejectTalk;
using ConferenceExample.Conference.Application.RenameConference;
using ConferenceExample.Conference.Application.ScheduleTalk;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;
using ConferenceExample.Conference.Persistence;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Application;
using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Application.SubmitTalk;
using ConferenceExample.Talk.Domain.TalkManagement;
using ConferenceExample.Talk.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace ConferenceExample.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Configure MongoDB Event Store
        var mongoSettings =
            configuration.GetSection("EventStore:MongoDB").Get<MongoDbSettings>()
            ?? new MongoDbSettings(); // Use defaults if not configured

        var mongoClient = new MongoClient(mongoSettings.ConnectionString);
        var database = mongoClient.GetDatabase(mongoSettings.DatabaseName);

        services.AddSingleton(database);
        services.AddSingleton<IEventStore, MongoDbEventStore>();
        services.AddSingleton<IEventBus, MongoDbEventBus>();

        services.AddScoped<IConferenceRepository, ConferenceRepository>();
        services.AddScoped<ITalkRepository, TalkRepository>();

        services.AddScoped<ICreateConferenceCommandHandler, CreateConferenceCommandHandler>();
        services.AddScoped<IRenameConferenceCommandHandler, RenameConferenceCommandHandler>();
        services.AddScoped<IAcceptTalkCommandHandler, AcceptTalkCommandHandler>();
        services.AddScoped<IRejectTalkCommandHandler, RejectTalkCommandHandler>();
        services.AddScoped<IScheduleTalkCommandHandler, ScheduleTalkCommandHandler>();
        services.AddScoped<IAssignTalkToRoomCommandHandler, AssignTalkToRoomCommandHandler>();
        services.AddScoped<ISubmitTalkCommandHandler, SubmitTalkCommandHandler>();
        services.AddScoped<IEditTalkCommandHandler, EditTalkCommandHandler>();
        services.AddScoped<IGetMyTalksQueryHandler, GetMyTalksQueryHandler>();
        services.AddScoped<IGetAllConferencesQueryHandler, GetAllConferencesQueryHandler>();
        services.AddScoped<IGetConferenceByIdQueryHandler, GetConferenceByIdQueryHandler>();
        services.AddScoped<IGetConferenceSessionsQueryHandler, GetConferenceSessionsQueryHandler>();
        services.AddScoped<IGetConferenceTalksQueryHandler, GetConferenceTalksQueryHandler>();

        services.AddScoped<IConferenceService, ConferenceService>();
        services.AddScoped<ITalkService, TalkService>();

        return services;
    }

    public static IServiceCollection AddAuthenticationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var jwtSettings =
            configuration.GetSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings not found in configuration");

        services.AddSingleton(jwtSettings);
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret)
                    ),
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static WebApplication AddEventBusSubscriptions(this WebApplication app)
    {
        var eventBus = app.Services.GetRequiredService<IEventBus>();
        var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

        eventBus.Subscribe(
            "TalkSubmittedEvent",
            storedEvent =>
            {
                var payload = JsonSerializer.Deserialize<TalkSubmittedPayload>(storedEvent.Payload);
                if (payload is null)
                    return;

                using var scope = scopeFactory.CreateScope();
                var conferenceRepo =
                    scope.ServiceProvider.GetRequiredService<IConferenceRepository>();
                var conference = conferenceRepo
                    .GetById(
                        new Conference.Domain.ConferenceManagement.ConferenceId(
                            new GuidV7(payload.ConferenceId)
                        )
                    )
                    .Result;
                conference.SubmitTalk(
                    new Conference.Domain.TalkManagement.TalkId(new GuidV7(storedEvent.AggregateId))
                );
                conferenceRepo.Save(conference).Wait();
            }
        );

        return app;
    }

    private record TalkSubmittedPayload(Guid ConferenceId);
}
