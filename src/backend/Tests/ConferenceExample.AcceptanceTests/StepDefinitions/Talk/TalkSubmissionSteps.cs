using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ConferenceExample.AcceptanceTests.Infrastructure;
using ConferenceExample.API.Controllers;
using ConferenceExample.Authentication;
using ConferenceExample.Conference.Application.ChangeConferenceStatus;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.DefineTalkType;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Talk.Application.CreateSpeakerProfile;
using ConferenceExample.Talk.Application.GetTalkById;
using ConferenceExample.Talk.Application.SubmitTalk;
using ConferenceExample.Talk.Domain.SharedKernel;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Persistence.ReadModels;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Xunit;

namespace ConferenceExample.AcceptanceTests.StepDefinitions.Talk;

[Binding]
public class TalkSubmissionSteps(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions ResponseJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private Guid _conferenceId;
    private Guid _talkTypeId;
    private Guid _talkId;
    private string _submittedTitle = string.Empty;
    private string _submittedAbstract = string.Empty;
    private string _organizerToken = string.Empty;
    private string _speakerToken = string.Empty;
    private string _otherSpeakerToken = string.Empty;
    private HttpResponseMessage _submitResponse = null!;

    [Given("an organizer is registered")]
    public async Task GivenAnOrganizerIsRegistered()
    {
        _organizerToken = await Register(UserRole.Organizer);
    }

    [Given("a speaker is registered")]
    public async Task GivenASpeakerIsRegistered()
    {
        _speakerToken = await Register(UserRole.Speaker);
    }

    [Given("another speaker is registered")]
    public async Task GivenAnotherSpeakerIsRegistered()
    {
        _otherSpeakerToken = await Register(UserRole.Speaker);
    }

    [Given("a conference exists")]
    public async Task GivenAConferenceExists()
    {
        SetBearerToken(_organizerToken);

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
        _conferenceId = conference.Id;

        var talkTypeResponse = await httpClient.PostAsJsonAsync(
            $"/api/conferences/{_conferenceId}/talk-types",
            new DefineTalkTypeDto("Session", 30)
        );
        Assert.Equal(HttpStatusCode.Created, talkTypeResponse.StatusCode);
        var talkType = await talkTypeResponse.Content.ReadFromJsonAsync<TalkTypeDefinedDto>(
            ResponseJsonOptions
        );
        Assert.NotNull(talkType);
        _talkTypeId = talkType.TalkTypeId;

        var statusResponse = await httpClient.PutAsJsonAsync(
            $"/api/conferences/{_conferenceId}/status",
            new ChangeConferenceStatusDto { Status = ConferenceStatus.CallForSpeakers }
        );
        Assert.Equal(HttpStatusCode.NoContent, statusResponse.StatusCode);

        // The Talk BC only learns about the status change once ConferenceStatusChangedEvent is
        // replicated into its local event store via the event bus — poll rather than assume it
        // has landed by the time this method returns.
        await WaitForConferenceReadyForSubmissions();

        ClearBearerToken();
    }

    [Given("a conference exists that is not yet accepting submissions")]
    public async Task GivenAConferenceExistsThatIsNotYetAcceptingSubmissions()
    {
        SetBearerToken(_organizerToken);

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
        _conferenceId = conference.Id;

        var talkTypeResponse = await httpClient.PostAsJsonAsync(
            $"/api/conferences/{_conferenceId}/talk-types",
            new DefineTalkTypeDto("Session", 30)
        );
        Assert.Equal(HttpStatusCode.Created, talkTypeResponse.StatusCode);
        var talkType = await talkTypeResponse.Content.ReadFromJsonAsync<TalkTypeDefinedDto>(
            ResponseJsonOptions
        );
        Assert.NotNull(talkType);
        _talkTypeId = talkType.TalkTypeId;

        // Left in Draft status on purpose — the conference exists but isn't accepting talk
        // submissions yet. Still wait for it to be replicated into the Talk BC's local event
        // store, same reason as WaitForConferenceReadyForSubmissions, just without requiring
        // CanAcceptTalkSubmissions() to be true.
        await WaitForConferenceToExist();

        ClearBearerToken();
    }

    [When("the speaker submits a talk for a nonexistent conference")]
    public async Task WhenTheSpeakerSubmitsATalkForANonexistentConference()
    {
        _conferenceId = Guid.CreateVersion7();
        _talkTypeId = Guid.CreateVersion7();
        await SubmitTalk("Introduction to DDD", "An overview of Domain-Driven Design", []);
    }

    [Then("the submission is rejected with status {int}")]
    public void ThenTheSubmissionIsRejectedWithStatus(int expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, (int)_submitResponse.StatusCode);
    }

    [When("the speaker submits a talk titled {string} with abstract {string}")]
    public async Task WhenTheSpeakerSubmitsATalk(string title, string @abstract)
    {
        await SubmitTalk(title, @abstract, []);
    }

    [When(
        "the speaker submits a talk titled {string} with abstract {string} tagged {string} and {string}"
    )]
    public async Task WhenTheSpeakerSubmitsATalkWithTags(
        string title,
        string @abstract,
        string tag1,
        string tag2
    )
    {
        await SubmitTalk(title, @abstract, [tag1, tag2]);
    }

    [Then("the talk is stored with status Submitted")]
    public async Task ThenTheTalkIsStoredWithStatusSubmitted()
    {
        var document = await WaitForTalkDocument();
        Assert.Equal("Submitted", document.Status);
        Assert.Equal(_submittedTitle, document.Title);
        Assert.Equal(_submittedAbstract, document.Abstract);

        var response = await WaitForTalkResponse();
        Assert.Equal("Submitted", response.Status);
        Assert.Equal(_submittedTitle, response.Title);
        Assert.Equal(_submittedAbstract, response.Abstract);
    }

    [Then("the organizer can view the talk")]
    public async Task ThenTheOrganizerCanViewTheTalk()
    {
        await WaitForTalkDocument();

        SetBearerToken(_organizerToken);

        // The ConferenceId -> OrganizerId projection used to authorize this is populated
        // asynchronously from a separate event subscription, so poll rather than assert once.
        var deadline = DateTimeOffset.UtcNow.AddSeconds(5);
        HttpResponseMessage response;
        do
        {
            response = await httpClient.GetAsync($"/api/talks/{_talkId}");
            if (response.StatusCode == HttpStatusCode.OK)
                return;

            await Task.Delay(50);
        } while (DateTimeOffset.UtcNow < deadline);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Then("the other speaker cannot view the talk")]
    public async Task ThenTheOtherSpeakerCannotViewTheTalk()
    {
        await WaitForTalkDocument();

        SetBearerToken(_otherSpeakerToken);

        var response = await httpClient.GetAsync($"/api/talks/{_talkId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Then("the talk has the tag {string}")]
    public async Task ThenTheTalkHasTheTag(string expectedTag)
    {
        var document = await WaitForTalkDocument();
        Assert.Contains(expectedTag, document.Tags);

        var response = await WaitForTalkResponse();
        Assert.Contains(expectedTag, response.Tags);
    }

    private async Task SubmitTalk(string title, string @abstract, List<string> tags)
    {
        _submittedTitle = title;
        _submittedAbstract = @abstract;

        SetBearerToken(_speakerToken);

        var profileResponse = await httpClient.PostAsJsonAsync(
            "/api/speakers/profile",
            new CreateSpeakerProfileDto
            {
                FirstName = "Jane",
                LastName = "Doe",
                Biography = "Test speaker biography.",
            }
        );
        Assert.Equal(HttpStatusCode.Created, profileResponse.StatusCode);

        _submitResponse = await httpClient.PostAsJsonAsync(
            "/api/talks",
            new SubmitTalkDto
            {
                Title = title,
                Abstract = @abstract,
                ConferenceId = _conferenceId,
                Tags = tags,
                TalkTypeId = _talkTypeId,
            }
        );

        if (_submitResponse.StatusCode != HttpStatusCode.Created)
        {
            return;
        }

        var location =
            _submitResponse.Headers.Location?.ToString()
            ?? throw new InvalidOperationException(
                "Talk submission response did not include a Location header."
            );
        _talkId = Guid.Parse(location.Split('/').Last());
    }

    // Talk read models are projected asynchronously from stored events (see InMemoryEventBus),
    // so the document can be missing for a moment right after submission — poll instead of
    // asserting once. Reading the read-model repository directly (rather than the API's GET
    // endpoint) verifies the data actually landed in the database, not just what the API layer
    // returns.
    private async Task<TalkDocument> WaitForTalkDocument()
    {
        using var scope = AcceptanceTestEnvironment.Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITalkDocumentRepository>();

        var deadline = DateTimeOffset.UtcNow.AddSeconds(5);
        while (DateTimeOffset.UtcNow < deadline)
        {
            var document = await repository.GetById(_talkId);
            if (document is not null)
                return document;

            await Task.Delay(50);
        }

        throw new TimeoutException(
            $"Talk {_talkId} did not appear in the database within the timeout."
        );
    }

    // Same eventual-consistency caveat as WaitForTalkDocument, but through the GET endpoint —
    // this verifies the API's own read path (routing, controller, DTO mapping) returns the
    // talk correctly, which the database check above does not cover.
    private async Task<GetTalkByIdDto> WaitForTalkResponse()
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(5);
        while (DateTimeOffset.UtcNow < deadline)
        {
            var response = await httpClient.GetAsync($"/api/talks/{_talkId}");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var talk = await response.Content.ReadFromJsonAsync<GetTalkByIdDto>(
                    ResponseJsonOptions
                );
                Assert.NotNull(talk);
                return talk;
            }

            await Task.Delay(50);
        }

        throw new TimeoutException(
            $"Talk {_talkId} did not appear in the read model within the timeout."
        );
    }

    private async Task WaitForConferenceReadyForSubmissions()
    {
        using var scope = AcceptanceTestEnvironment.Factory.Services.CreateScope();
        var repository =
            scope.ServiceProvider.GetRequiredService<ConferenceExample.Talk.Domain.TalkManagement.IConferenceRepository>();
        var conferenceId = new ConferenceExample.Talk.Domain.TalkManagement.ConferenceId(
            new GuidV7(_conferenceId)
        );

        var deadline = DateTimeOffset.UtcNow.AddSeconds(5);
        while (DateTimeOffset.UtcNow < deadline)
        {
            try
            {
                var conference = await repository.GetById(conferenceId);
                if (conference.CanAcceptTalkSubmissions())
                    return;
            }
            catch (NotFoundException)
            {
                // Not yet replicated into the Talk BC's local event store.
            }

            await Task.Delay(50);
        }

        throw new TimeoutException(
            $"Conference {_conferenceId} did not become ready for talk submissions within the timeout."
        );
    }

    private async Task WaitForConferenceToExist()
    {
        using var scope = AcceptanceTestEnvironment.Factory.Services.CreateScope();
        var repository =
            scope.ServiceProvider.GetRequiredService<ConferenceExample.Talk.Domain.TalkManagement.IConferenceRepository>();
        var conferenceId = new ConferenceExample.Talk.Domain.TalkManagement.ConferenceId(
            new GuidV7(_conferenceId)
        );

        var deadline = DateTimeOffset.UtcNow.AddSeconds(5);
        while (DateTimeOffset.UtcNow < deadline)
        {
            try
            {
                await repository.GetById(conferenceId);
                return;
            }
            catch (NotFoundException)
            {
                // Not yet replicated into the Talk BC's local event store.
            }

            await Task.Delay(50);
        }

        throw new TimeoutException(
            $"Conference {_conferenceId} did not appear in the Talk BC's local event store within the timeout."
        );
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

    private void SetBearerToken(string token)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );
    }

    private void ClearBearerToken()
    {
        httpClient.DefaultRequestHeaders.Authorization = null;
    }
}
