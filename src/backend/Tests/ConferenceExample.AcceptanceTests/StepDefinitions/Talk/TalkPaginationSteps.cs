using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ConferenceExample.AcceptanceTests.Infrastructure;
using ConferenceExample.API.Controllers;
using ConferenceExample.Talk.Application.CreateSpeakerProfile;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Application.SubmitTalk;
using ConferenceExample.Talk.Persistence.ReadModels;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Xunit;

namespace ConferenceExample.AcceptanceTests.StepDefinitions.Talk;

[Binding]
public class TalkPaginationSteps(HttpClient httpClient, TalkSubmissionSteps talkSubmissionSteps)
{
    private static readonly JsonSerializerOptions ResponseJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private bool _speakerProfileCreated;
    private HttpResponseMessage _response = null!;
    private PagedResult<GetMyTalksDto>? _pagedResult;

    [Given("the speaker has submitted {int} talks")]
    public async Task GivenTheSpeakerHasSubmittedTalks(int count)
    {
        for (var i = 0; i < count; i++)
        {
            await SubmitTalk($"Talk {i + 1}", "An abstract.");
        }
    }

    [When("the speaker requests their talks with page {int} and page size {int}")]
    public async Task WhenTheSpeakerRequestsTheirTalks(int page, int pageSize)
    {
        SetBearerToken(talkSubmissionSteps.SpeakerToken);

        _response = await httpClient.GetAsync(
            $"/api/talks/my-talks?page={page}&pageSize={pageSize}"
        );

        if (_response.StatusCode == HttpStatusCode.OK)
        {
            _pagedResult = await _response.Content.ReadFromJsonAsync<PagedResult<GetMyTalksDto>>(
                ResponseJsonOptions
            );
        }
    }

    [Then("the response contains {int} talk")]
    [Then("the response contains {int} talks")]
    public void ThenTheResponseContainsTalks(int expectedCount)
    {
        Assert.Equal(HttpStatusCode.OK, _response.StatusCode);
        Assert.NotNull(_pagedResult);
        Assert.Equal(expectedCount, _pagedResult.Items.Count);
    }

    [Then("the total count is {int}")]
    public void ThenTheTotalCountIs(int expectedTotalCount)
    {
        Assert.NotNull(_pagedResult);
        Assert.Equal(expectedTotalCount, _pagedResult.TotalCount);
    }

    [Then("the pagination request is rejected with status {int}")]
    public void ThenThePaginationRequestIsRejectedWithStatus(int expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, (int)_response.StatusCode);
    }

    private async Task SubmitTalk(string title, string @abstract)
    {
        SetBearerToken(talkSubmissionSteps.SpeakerToken);

        if (!_speakerProfileCreated)
        {
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
            _speakerProfileCreated = true;
        }

        var submitResponse = await httpClient.PostAsJsonAsync(
            "/api/talks",
            new SubmitTalkDto
            {
                Title = title,
                Abstract = @abstract,
                ConferenceId = talkSubmissionSteps.ConferenceId,
                Tags = [],
                TalkTypeId = talkSubmissionSteps.TalkTypeId,
            }
        );
        Assert.Equal(HttpStatusCode.Created, submitResponse.StatusCode);

        var location =
            submitResponse.Headers.Location?.ToString()
            ?? throw new InvalidOperationException(
                "Talk submission response did not include a Location header."
            );
        var talkId = Guid.Parse(location.Split('/').Last());

        // Talk read models are projected asynchronously from stored events, so wait for this
        // talk to land before submitting the next one or querying the paginated list — otherwise
        // the total count seen by GetMyTalks would be flaky depending on projection timing.
        await WaitForTalkDocument(talkId);
    }

    private static async Task WaitForTalkDocument(Guid talkId)
    {
        using var scope = AcceptanceTestEnvironment.Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITalkDocumentRepository>();

        var deadline = DateTimeOffset.UtcNow.AddSeconds(5);
        while (DateTimeOffset.UtcNow < deadline)
        {
            var document = await repository.GetById(talkId);
            if (document is not null)
                return;

            await Task.Delay(50);
        }

        throw new TimeoutException(
            $"Talk {talkId} did not appear in the database within the timeout."
        );
    }

    private void SetBearerToken(string token)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );
    }
}
