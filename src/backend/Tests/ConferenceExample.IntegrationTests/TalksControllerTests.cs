using System.Net;
using System.Net.Http.Json;
using ConferenceExample.Authentication;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.IntegrationTests.Infrastructure;
using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Application.SubmitTalk;

namespace ConferenceExample.IntegrationTests;

[Collection("IntegrationTests")]
public class TalksControllerTests : IntegrationTestBase
{
    public TalksControllerTests(IntegrationTestWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task SubmitTalk_WithValidData_ReturnsCreated()
    {
        // Arrange
        var conferenceId = await CreateConferenceAsync();
        var token = await GetAuthenticationToken(
            GetUniqueEmail("speaker"),
            "password123",
            UserRole.Speaker
        );
        SetAuthenticationToken(token);

        var submitDto = new SubmitTalkDto
        {
            Title = "Clean Architecture in Practice",
            Abstract =
                "A deep dive into applying Clean Architecture principles in a real-world .NET application.",
            ConferenceId = conferenceId,
            TalkTypeId = Guid.CreateVersion7(),
            Tags = new List<string> { "dotnet", "architecture", "ddd" },
        };

        // Act
        var response = await PostAsync("/api/talks", submitDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task SubmitTalk_WithoutAuthorization_ReturnsUnauthorized()
    {
        // Arrange
        var submitDto = new SubmitTalkDto
        {
            Title = "Test Talk",
            Abstract = "Test Abstract",
            ConferenceId = Guid.NewGuid(),
            TalkTypeId = Guid.NewGuid(),
            Tags = new List<string> { "test" },
        };

        // Act
        var response = await PostAsync("/api/talks", submitDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMyTalks_ReturnsEmptyList_WhenNoTalksExist()
    {
        // Arrange
        var token = await GetAuthenticationToken(
            GetUniqueEmail("speaker"),
            "password123",
            UserRole.Speaker
        );
        SetAuthenticationToken(token);

        // Act
        var response = await HttpClient.GetAsync("/api/talks/my-talks");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<GetMyTalksDto>>();
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMyTalks_ReturnsTalks_WhenTalksExist()
    {
        // Arrange
        var conferenceId = await CreateConferenceAsync();
        var token = await GetAuthenticationToken(
            GetUniqueEmail("speaker"),
            "password123",
            UserRole.Speaker
        );
        SetAuthenticationToken(token);

        var submitDto = new SubmitTalkDto
        {
            Title = "My Test Talk",
            Abstract = "Test Abstract for my talk",
            ConferenceId = conferenceId,
            TalkTypeId = Guid.CreateVersion7(),
            Tags = new List<string> { "test", "integration" },
        };
        var test = await PostAsync("/api/talks", submitDto);

        // Act
        var response = await HttpClient.GetAsync("/api/talks/my-talks");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<GetMyTalksDto>>();
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("My Test Talk", result![0].Title);
        Assert.Equal("Test Abstract for my talk", result[0].Abstract);
        Assert.Equal(new[] { "test", "integration" }, result[0].Tags);
    }

    [Fact]
    public async Task EditTalk_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var conferenceId = await CreateConferenceAsync();
        var token = await GetAuthenticationToken(
            GetUniqueEmail("speaker"),
            "password123",
            UserRole.Speaker
        );
        SetAuthenticationToken(token);

        // Create a talk first
        var submitDto = new SubmitTalkDto
        {
            Title = "Original Title",
            Abstract = "Original Abstract",
            ConferenceId = conferenceId,
            TalkTypeId = Guid.CreateVersion7(),
            Tags = new List<string> { "original" },
        };
        await PostAsync("/api/talks", submitDto);

        // Get the talk ID
        var getResponse = await HttpClient.GetAsync("/api/talks/my-talks");
        var talks = await getResponse.Content.ReadFromJsonAsync<List<GetMyTalksDto>>();
        var talkId = talks![0].Id;

        var editDto = new EditTalkDto(
            "Updated Title",
            "Updated Abstract",
            new List<string> { "updated", "edited" }
        );

        // Act
        var response = await PutAsync($"/api/talks/{talkId}", editDto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the talk was updated
    }

    private async Task<Guid> CreateConferenceAsync()
    {
        // Create a conference as organizer
        var currentToken = HttpClient.DefaultRequestHeaders.Authorization;

        var organizerToken = await GetAuthenticationToken(
            GetUniqueEmail("organizer"),
            "password123",
            UserRole.Organizer
        );
        SetAuthenticationToken(organizerToken);

        var createDto = new CreateConferenceDto
        {
            Name = "Test Conference for Talks",
            Start = new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 10, 3, 18, 0, 0, TimeSpan.Zero),
            LocationName = "Test Location",
            Street = "Test Street",
            City = "Test City",
            State = "Test State",
            PostalCode = "12345",
            Country = "Test Country",
        };

        var response = await PostAsync("/api/conferences", createDto);
        var result =
            await DeserializeResponse<Conference.Application.CreateConference.ConferenceCreatedDto>(
                response
            );

        // Change status to CallForSpeakers so talks can be submitted
        var changeStatusDto =
            new Conference.Application.ChangeConferenceStatus.ChangeConferenceStatusDto
            {
                Status = Conference.Domain.ConferenceManagement.ConferenceStatus.CallForSpeakers,
            };
        var statusResponse = await PutAsync(
            $"/api/conferences/{result!.Id}/status",
            changeStatusDto
        );
        statusResponse.EnsureSuccessStatusCode(); // Ensure status change succeeded

        // Restore previous token if any
        HttpClient.DefaultRequestHeaders.Authorization = currentToken;

        return result!.Id;
    }
}
