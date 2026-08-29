using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ConferenceExample.API.Controllers;
using ConferenceExample.Authentication;
using ConferenceExample.Conference.Application.ChangeConferenceStatus;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.DefineTalkType;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Talk.Application.CreateSpeakerProfile;
using ConferenceExample.Talk.Application.GetTalkById;
using ConferenceExample.Talk.Application.SubmitTalk;
using Reqnroll;
using Xunit;

namespace ConferenceExample.Talk.AcceptanceTests.StepDefinitions;

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

    [Given("a conference exists")]
    public async Task GivenAConferenceExists()
    {
        SetBearerToken(await Register(UserRole.Organizer));

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

        ClearBearerToken();
    }

    [When("a speaker submits a talk titled {string} with abstract {string}")]
    public async Task WhenASpeakerSubmitsATalk(string title, string @abstract)
    {
        await SubmitTalk(title, @abstract, []);
    }

    [When(
        "a speaker submits a talk titled {string} with abstract {string} tagged {string} and {string}"
    )]
    public async Task WhenASpeakerSubmitsATalkWithTags(
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
        var talk = await WaitForTalk();
        Assert.Equal("Submitted", talk.Status);
    }

    [Then("the talk has the tag {string}")]
    public async Task ThenTheTalkHasTheTag(string expectedTag)
    {
        var talk = await WaitForTalk();
        Assert.Contains(expectedTag, talk.Tags);
    }

    private async Task SubmitTalk(string title, string @abstract, List<string> tags)
    {
        SetBearerToken(await Register(UserRole.Speaker));

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

        var submitResponse = await httpClient.PostAsJsonAsync(
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
        Assert.Equal(HttpStatusCode.Created, submitResponse.StatusCode);

        var location =
            submitResponse.Headers.Location?.ToString()
            ?? throw new InvalidOperationException(
                "Talk submission response did not include a Location header."
            );
        _talkId = Guid.Parse(location.Split('/').Last());

        ClearBearerToken();
    }

    // Talk read models are projected asynchronously from stored events (see InMemoryEventBus),
    // so the GET can 404 for a moment right after submission — poll instead of asserting once.
    private async Task<GetTalkByIdDto> WaitForTalk()
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
