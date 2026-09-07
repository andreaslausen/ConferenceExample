using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ConferenceExample.AcceptanceTests.Infrastructure;
using ConferenceExample.API.Controllers;
using ConferenceExample.Authentication;
using ConferenceExample.Speaker.Application.CreateSpeakerProfile;
using ConferenceExample.Speaker.Application.GetMyProfile;
using ConferenceExample.Speaker.Application.UpdateSpeakerProfile;
using ConferenceExample.Speaker.Persistence.ReadModels;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Xunit;

namespace ConferenceExample.AcceptanceTests.StepDefinitions.Speaker;

[Binding]
public class SpeakerSteps(HttpClient httpClient, ScenarioState state)
{
    private static readonly JsonSerializerOptions ResponseJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private Guid _speakerId;

    [Given("an organizer is registered")]
    public async Task GivenAnOrganizerIsRegistered()
    {
        state.OrganizerToken = await Register(UserRole.Organizer);
    }

    [Given("a speaker is registered")]
    public async Task GivenASpeakerIsRegistered()
    {
        state.SpeakerToken = await Register(UserRole.Speaker);
    }

    [Given("another speaker is registered")]
    public async Task GivenAnotherSpeakerIsRegistered()
    {
        state.OtherSpeakerToken = await Register(UserRole.Speaker);
    }

    [Given("the speaker has a profile")]
    public async Task GivenTheSpeakerHasAProfile()
    {
        state.SignInAsSpeaker();
        _speakerId = await CreateProfile("Jane", "Doe", "Test speaker biography.");
    }

    [Given("the other speaker has a profile")]
    public async Task GivenTheOtherSpeakerHasAProfile()
    {
        state.SignInAsOtherSpeaker();
        await CreateProfile("John", "Smith", "Another speaker biography.");
    }

    [When("the speaker creates a profile named {string} {string}")]
    public async Task WhenTheSpeakerCreatesAProfile(string firstName, string lastName)
    {
        state.SignInAsSpeaker();

        state.LastResponse = await httpClient.PostAsJsonAsync(
            "/api/speakers/profile",
            new CreateSpeakerProfileDto(firstName, lastName, "Test speaker biography.")
        );

        if (state.LastResponse.StatusCode != HttpStatusCode.Created)
            return;

        var created = await state.LastResponse.Content.ReadFromJsonAsync<SpeakerProfileCreatedDto>(
            ResponseJsonOptions
        );
        Assert.NotNull(created);
        _speakerId = created.SpeakerId;
    }

    [When("the speaker creates a second profile")]
    public async Task WhenTheSpeakerCreatesASecondProfile()
    {
        state.SignInAsSpeaker();

        state.LastResponse = await httpClient.PostAsJsonAsync(
            "/api/speakers/profile",
            new CreateSpeakerProfileDto("Jane", "Doe", "Test speaker biography.")
        );
    }

    [When("the speaker renames their profile to {string} {string}")]
    public async Task WhenTheSpeakerRenamesTheirProfile(string firstName, string lastName)
    {
        state.SignInAsSpeaker();

        state.LastResponse = await httpClient.PutAsJsonAsync(
            "/api/speakers/profile",
            new UpdateSpeakerProfileDto(firstName, lastName, "Updated speaker biography.")
        );
        Assert.Equal(HttpStatusCode.NoContent, state.LastResponse.StatusCode);
    }

    [Then("the profile is stored for {string} {string}")]
    public async Task ThenTheProfileIsStoredFor(string firstName, string lastName)
    {
        // Straight from the database — confirms the projection actually landed in MongoDB.
        var document = await Eventually.Succeeds(
            async () =>
            {
                using var scope = AcceptanceTestEnvironment.Factory.Services.CreateScope();
                var repository =
                    scope.ServiceProvider.GetRequiredService<ISpeakerDocumentRepository>();
                var stored = await repository.GetById(_speakerId);
                return stored?.FirstName == firstName && stored.LastName == lastName
                    ? stored
                    : null;
            },
            $"Speaker profile {firstName} {lastName}"
        );
        Assert.Equal(firstName, document.FirstName);
        Assert.Equal(lastName, document.LastName);

        // And through the API's own read path — routing, controller and DTO mapping.
        state.SignInAsSpeaker();
        var profile = await Eventually.Succeeds(
            async () =>
            {
                var response = await httpClient.GetAsync("/api/speakers/profile");
                if (response.StatusCode != HttpStatusCode.OK)
                    return null;

                var body = await response.Content.ReadFromJsonAsync<GetMyProfileDto>(
                    ResponseJsonOptions
                );
                return body?.FirstName == firstName ? body : null;
            },
            $"GET /api/speakers/profile returning {firstName} {lastName}"
        );
        Assert.Equal(firstName, profile.FirstName);
        Assert.Equal(lastName, profile.LastName);
    }

    [Then("the speaker profile id differs from the account id")]
    public async Task ThenTheSpeakerProfileIdDiffersFromTheAccountId()
    {
        // The two identities are deliberately separate so one account can hold several role
        // profiles later on.
        var document = await Eventually.Succeeds(
            async () =>
            {
                using var scope = AcceptanceTestEnvironment.Factory.Services.CreateScope();
                var repository =
                    scope.ServiceProvider.GetRequiredService<ISpeakerDocumentRepository>();
                return await repository.GetById(_speakerId);
            },
            $"Speaker profile {_speakerId}"
        );

        Assert.NotEqual(document.Id, document.UserId);
    }

    [Then("the profile request is rejected with status {int}")]
    public void ThenTheProfileRequestIsRejectedWithStatus(int expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, (int)state.LastResponse.StatusCode);
    }

    private async Task<Guid> CreateProfile(string firstName, string lastName, string biography)
    {
        var response = await httpClient.PostAsJsonAsync(
            "/api/speakers/profile",
            new CreateSpeakerProfileDto(firstName, lastName, biography)
        );
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<SpeakerProfileCreatedDto>(
            ResponseJsonOptions
        );
        Assert.NotNull(created);
        return created.SpeakerId;
    }

    private async Task<string> Register(UserRole role)
    {
        var response = await httpClient.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequestDto($"{Guid.CreateVersion7():N}@test.com", "Passw0rd!1", role)
        );
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RegisterResponseDto>(
            ResponseJsonOptions
        );
        Assert.NotNull(result);
        return result.Token;
    }
}
