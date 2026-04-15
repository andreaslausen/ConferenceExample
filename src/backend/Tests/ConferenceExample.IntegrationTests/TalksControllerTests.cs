using System.Net;
using System.Net.Http.Json;
using ConferenceExample.Authentication;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.IntegrationTests.Infrastructure;
using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Application.SubmitTalk;
using FluentAssertions;

namespace ConferenceExample.IntegrationTests;

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
            "speaker@test.com",
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
        response.StatusCode.Should().Be(HttpStatusCode.Created);
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
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyTalks_ReturnsEmptyList_WhenNoTalksExist()
    {
        // Arrange
        var token = await GetAuthenticationToken(
            "speaker@test.com",
            "password123",
            UserRole.Speaker
        );
        SetAuthenticationToken(token);

        // Act
        var response = await HttpClient.GetAsync("/api/talks/my-talks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<GetMyTalksDto>>();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMyTalks_ReturnsTalks_WhenTalksExist()
    {
        // Arrange
        var conferenceId = await CreateConferenceAsync();
        var token = await GetAuthenticationToken(
            "speaker@test.com",
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
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<GetMyTalksDto>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result![0].Title.Should().Be("My Test Talk");
        result[0].Abstract.Should().Be("Test Abstract for my talk");
        result[0].Tags.Should().BeEquivalentTo(new[] { "test", "integration" });
    }

    [Fact]
    public async Task EditTalk_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var conferenceId = await CreateConferenceAsync();
        var token = await GetAuthenticationToken(
            "speaker@test.com",
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
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the talk was updated
    }

    private async Task<Guid> CreateConferenceAsync()
    {
        // Create a conference as organizer
        var currentToken = HttpClient.DefaultRequestHeaders.Authorization;

        var organizerToken = await GetAuthenticationToken(
            "organizer-for-talk@test.com",
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

        // Restore previous token if any
        HttpClient.DefaultRequestHeaders.Authorization = currentToken;

        return result!.Id;
    }
}
