using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ConferenceExample.AcceptanceTests.Infrastructure;
using ConferenceExample.Conference.Application.ChangeConferenceStatus;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.DefineTalkType;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using Reqnroll;
using Xunit;

namespace ConferenceExample.AcceptanceTests.StepDefinitions.Conference;

[Binding]
public class ConferenceSteps(HttpClient httpClient, ScenarioState state)
{
    private static readonly JsonSerializerOptions ResponseJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Given("a conference exists")]
    public async Task GivenAConferenceExists()
    {
        await CreateConferenceWithTalkType();

        var statusResponse = await httpClient.PutAsJsonAsync(
            $"/api/conferences/{state.ConferenceId}/status",
            new ChangeConferenceStatusDto { Status = ConferenceStatus.CallForSpeakers }
        );
        Assert.Equal(HttpStatusCode.NoContent, statusResponse.StatusCode);

        state.SignOut();
    }

    [Given("a conference exists that is not yet accepting submissions")]
    public async Task GivenAConferenceExistsThatIsNotYetAcceptingSubmissions()
    {
        // Left in Draft status on purpose — the conference exists but isn't accepting talk
        // submissions yet.
        await CreateConferenceWithTalkType();

        state.SignOut();
    }

    private async Task CreateConferenceWithTalkType()
    {
        state.SignInAsOrganizer();

        var createResponse = await httpClient.PostAsJsonAsync(
            "/api/conferences",
            new CreateConferenceDto
            {
                Name = "Test Conference",
                Start = DateTimeOffset.UtcNow.AddMonths(1),
                End = DateTimeOffset.UtcNow.AddMonths(1).AddDays(2),
                LocationName = "Test Location",
                Street = "123 Test St",
                City = "Test City",
                State = "Test State",
                PostalCode = "12345",
                Country = "Test Country",
            }
        );
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var conference = await createResponse.Content.ReadFromJsonAsync<ConferenceCreatedDto>(
            ResponseJsonOptions
        );
        Assert.NotNull(conference);
        state.ConferenceId = conference.Id;

        var talkTypeResponse = await httpClient.PostAsJsonAsync(
            $"/api/conferences/{state.ConferenceId}/talk-types",
            new DefineTalkTypeDto("Session", 30)
        );
        Assert.Equal(HttpStatusCode.Created, talkTypeResponse.StatusCode);

        var talkType = await talkTypeResponse.Content.ReadFromJsonAsync<TalkTypeDefinedDto>(
            ResponseJsonOptions
        );
        Assert.NotNull(talkType);
        state.TalkTypeId = talkType.TalkTypeId;
    }
}
