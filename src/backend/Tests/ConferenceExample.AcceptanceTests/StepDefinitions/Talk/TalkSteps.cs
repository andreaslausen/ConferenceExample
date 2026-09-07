using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ConferenceExample.AcceptanceTests.Infrastructure;
using ConferenceExample.Talk.Application.CreateTalk;
using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Application.GetTalkById;
using ConferenceExample.Talk.Persistence.ReadModels;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Xunit;

namespace ConferenceExample.AcceptanceTests.StepDefinitions.Talk;

/// <summary>
/// A speaker's own talks: created, edited and deleted without any conference being involved.
/// </summary>
[Binding]
public class TalkSteps(HttpClient httpClient, ScenarioState state)
{
    private static readonly JsonSerializerOptions ResponseJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Given("the speaker has a talk titled {string}")]
    public async Task GivenTheSpeakerHasATalkTitled(string title)
    {
        await CreateTalk(title, "An overview of Domain-Driven Design", []);
        Assert.Equal(HttpStatusCode.Created, state.LastResponse.StatusCode);
    }

    [When("the speaker creates a talk titled {string} with abstract {string}")]
    public async Task WhenTheSpeakerCreatesATalk(string title, string @abstract)
    {
        await CreateTalk(title, @abstract, []);
    }

    [When(
        "the speaker creates a talk titled {string} with abstract {string} tagged {string} and {string}"
    )]
    public async Task WhenTheSpeakerCreatesATalkWithTags(
        string title,
        string @abstract,
        string tag1,
        string tag2
    )
    {
        await CreateTalk(title, @abstract, [tag1, tag2]);
    }

    [When("the speaker renames the talk to {string}")]
    public async Task WhenTheSpeakerRenamesTheTalk(string title)
    {
        state.SignInAsSpeaker();

        state.LastResponse = await httpClient.PutAsJsonAsync(
            $"/api/talks/{state.TalkId}",
            new EditTalkDto(title, "An overview of Domain-Driven Design", [])
        );
        Assert.Equal(HttpStatusCode.NoContent, state.LastResponse.StatusCode);
    }

    [When("the speaker deletes the talk")]
    public async Task WhenTheSpeakerDeletesTheTalk()
    {
        state.SignInAsSpeaker();

        state.LastResponse = await httpClient.DeleteAsync($"/api/talks/{state.TalkId}");
    }

    [When("the other speaker tries to delete the talk")]
    public async Task WhenTheOtherSpeakerTriesToDeleteTheTalk()
    {
        state.SignInAsOtherSpeaker();

        state.LastResponse = await httpClient.DeleteAsync($"/api/talks/{state.TalkId}");
    }

    [Then("the talk request is rejected with status {int}")]
    public void ThenTheTalkRequestIsRejectedWithStatus(int expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, (int)state.LastResponse.StatusCode);
    }

    [Then("the talk request succeeds with status {int}")]
    public void ThenTheTalkRequestSucceedsWithStatus(int expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, (int)state.LastResponse.StatusCode);
    }

    [Then("the talk is stored titled {string} with abstract {string}")]
    public async Task ThenTheTalkIsStored(string title, string @abstract)
    {
        var document = await WaitForTalkDocument(d => d.Title == title);
        Assert.Equal(title, document.Title);
        Assert.Equal(@abstract, document.Abstract);

        var talk = await WaitForTalkResponse(t => t.Title == title);
        Assert.Equal(title, talk.Title);
        Assert.Equal(@abstract, talk.Abstract);
    }

    [Then("the talk has the tag {string}")]
    public async Task ThenTheTalkHasTheTag(string expectedTag)
    {
        var document = await WaitForTalkDocument(d => d.Tags.Contains(expectedTag));
        Assert.Contains(expectedTag, document.Tags);

        var talk = await WaitForTalkResponse(t => t.Tags.Contains(expectedTag));
        Assert.Contains(expectedTag, talk.Tags);
    }

    [Then("the talk is gone")]
    public async Task ThenTheTalkIsGone()
    {
        await Eventually.Succeeds(
            async () =>
            {
                using var scope = AcceptanceTestEnvironment.Factory.Services.CreateScope();
                var repository =
                    scope.ServiceProvider.GetRequiredService<ITalkDocumentRepository>();
                return await repository.GetById(state.TalkId) is null ? "gone" : null;
            },
            $"Talk {state.TalkId} disappearing from the database"
        );

        state.SignInAsSpeaker();
        var response = await httpClient.GetAsync($"/api/talks/{state.TalkId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Then("the other speaker cannot view the talk")]
    public async Task ThenTheOtherSpeakerCannotViewTheTalk()
    {
        await WaitForTalkDocument(_ => true);

        state.SignInAsOtherSpeaker();

        var response = await httpClient.GetAsync($"/api/talks/{state.TalkId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task CreateTalk(string title, string @abstract, List<string> tags)
    {
        state.SignInAsSpeaker();

        state.LastResponse = await httpClient.PostAsJsonAsync(
            "/api/talks",
            new CreateTalkDto(title, @abstract, tags)
        );

        if (state.LastResponse.StatusCode != HttpStatusCode.Created)
            return;

        var location =
            state.LastResponse.Headers.Location?.ToString()
            ?? throw new InvalidOperationException(
                "Talk creation response did not include a Location header."
            );
        state.TalkId = Guid.Parse(location.Split('/').Last());
    }

    // Reading the read-model repository directly (rather than the API's GET endpoint) verifies
    // the data actually landed in the database, not just what the API layer returns.
    private Task<TalkDocument> WaitForTalkDocument(Func<TalkDocument, bool> matches) =>
        Eventually.Succeeds(
            async () =>
            {
                using var scope = AcceptanceTestEnvironment.Factory.Services.CreateScope();
                var repository =
                    scope.ServiceProvider.GetRequiredService<ITalkDocumentRepository>();
                var document = await repository.GetById(state.TalkId);
                return document is not null && matches(document) ? document : null;
            },
            $"Talk {state.TalkId} appearing in the database"
        );

    // Same eventual-consistency caveat, but through the GET endpoint — this verifies the API's
    // own read path (routing, controller, DTO mapping), which the database check does not cover.
    private Task<GetTalkByIdDto> WaitForTalkResponse(Func<GetTalkByIdDto, bool> matches)
    {
        state.SignInAsSpeaker();

        return Eventually.Succeeds(
            async () =>
            {
                var response = await httpClient.GetAsync($"/api/talks/{state.TalkId}");
                if (response.StatusCode != HttpStatusCode.OK)
                    return null;

                var talk = await response.Content.ReadFromJsonAsync<GetTalkByIdDto>(
                    ResponseJsonOptions
                );
                return talk is not null && matches(talk) ? talk : null;
            },
            $"GET /api/talks/{state.TalkId} returning the expected talk"
        );
    }
}
