using ConferenceExample.API.Extensions;
using ConferenceExample.Authentication;
using ConferenceExample.Conference.Application;
using ConferenceExample.Conference.Persistence;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Application;
using ConferenceExample.Talk.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEventStore(builder.Configuration);
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddConferencePersistence();
builder.Services.AddConferenceApplication();
builder.Services.AddTalkPersistence();
builder.Services.AddTalkApplication();

builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddDefaultPolicy(policy =>
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
        );
    }
    else
    {
        var frontendUrl = builder.Configuration["Frontend:Url"] ?? "https://localhost:5173";
        options.AddDefaultPolicy(policy =>
            policy.WithOrigins(frontendUrl).AllowAnyMethod().AllowAnyHeader()
        );
    }
});

var app = builder.Build();
app.AddEventBusSubscriptions();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
