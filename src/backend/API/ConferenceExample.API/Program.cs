using ConferenceExample.API.Extensions;
using ConferenceExample.Authentication;
using ConferenceExample.Conference.Application;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEventStore(builder.Configuration);
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddConferenceContext();
builder.Services.AddTalkContext();

var app = builder.Build();
app.AddEventBusSubscriptions();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
