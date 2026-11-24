using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ConferenceExample.AcceptanceTests.Infrastructure;
using ConferenceExample.API.Controllers;
using ConferenceExample.Talk.Application.CreateTalk;
using ConferenceExample.Talk.Application.GetMyTalks;
using Reqnroll;
using Xunit;

namespace ConferenceExample.AcceptanceTests.StepDefinitions.Talk;

[Binding]
public class TalkPaginationSteps(HttpClient httpClient, ScenarioState state)
{
    private static readonly JsonSerializerOptions ResponseJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private PagedResult<GetMyTalksDto>? _pagedResult;
    private HttpResponseMessage _listResponse = null!;

    [Given("the speaker has created {int} talks")]
    public async Task GivenTheSpeakerHasCreatedTalks(int count)
    {
        state.SignInAsSpeaker();

        for (var i = 1; i <= count; i++)
        {
            var response = await httpClient.PostAsJsonAsync(
                "/api/talks",
                new CreateTalkDto($"Talk {i}", $"Abstract for talk {i}", [])
            );
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
    }

    [When("the speaker requests their talks with page {int} and page size {int}")]
    public async Task WhenTheSpeakerRequestsTheirTalks(int page, int pageSize)
    {
        state.SignInAsSpeaker();

        // The talk list is a projection, so a talk created a moment ago may not be listed yet —
        // poll until the page's total count has caught up with what was created.
        _listResponse = await httpClient.GetAsync(
            $"/api/talks/my-talks?page={page}&pageSize={pageSize}"
        );

        if (_listResponse.StatusCode != HttpStatusCode.OK)
        {
            _pagedResult = null;
            return;
        }

        _pagedResult = await _listResponse.Content.ReadFromJsonAsync<PagedResult<GetMyTalksDto>>(
            ResponseJsonOptions
        );
    }

    [When(
        "the speaker requests their talks with page {int} and page size {int} once {int} talks are listed"
    )]
    public async Task WhenTheSpeakerRequestsTheirTalksOnceListed(
        int page,
        int pageSize,
        int expectedTotalCount
    )
    {
        state.SignInAsSpeaker();

        _pagedResult = await Eventually.Succeeds(
            async () =>
            {
                _listResponse = await httpClient.GetAsync(
                    $"/api/talks/my-talks?page={page}&pageSize={pageSize}"
                );
                if (_listResponse.StatusCode != HttpStatusCode.OK)
                    return null;

                var result = await _listResponse.Content.ReadFromJsonAsync<
                    PagedResult<GetMyTalksDto>
                >(ResponseJsonOptions);
                return result?.TotalCount == expectedTotalCount ? result : null;
            },
            $"my-talks listing {expectedTotalCount} talks"
        );
    }

    [Then("the response contains {int} talk(s)")]
    public void ThenTheResponseContainsTalks(int expectedCount)
    {
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
        Assert.Equal(expectedStatusCode, (int)_listResponse.StatusCode);
    }
}
