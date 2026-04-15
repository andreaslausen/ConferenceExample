using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ConferenceExample.Authentication;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace ConferenceExample.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase
    : IClassFixture<IntegrationTestWebApplicationFactory>,
        IAsyncLifetime
{
    protected readonly IntegrationTestWebApplicationFactory Factory;
    protected HttpClient HttpClient { get; private set; } = null!;
    private IServiceScope? _scope;

    protected IntegrationTestBase(IntegrationTestWebApplicationFactory factory)
    {
        Factory = factory;
    }

    public virtual async Task InitializeAsync()
    {
        HttpClient = Factory.CreateClient();
        _scope = Factory.Services.CreateScope();

        // Clean database before each test
        await CleanDatabase();
    }

    public virtual async Task DisposeAsync()
    {
        _scope?.Dispose();
        HttpClient?.Dispose();
        await Task.CompletedTask;
    }

    protected async Task CleanDatabase()
    {
        var scope = Factory.Services.CreateScope();
        var mongoDatabase =
            scope.ServiceProvider.GetRequiredService<MongoDB.Driver.IMongoDatabase>();

        // Drop all collections
        var collectionsCursor = await mongoDatabase.ListCollectionNamesAsync();
        var collections = await collectionsCursor.ToListAsync();

        foreach (var collectionName in collections)
        {
            await mongoDatabase.DropCollectionAsync(collectionName);
        }

        scope.Dispose();
    }

    protected async Task<string> GetAuthenticationToken(
        string email,
        string password,
        UserRole role
    )
    {
        // Register user - use explicit JSON with role as integer (enum value)
        var registerJson =
            $@"{{
            ""email"": ""{email}"",
            ""password"": ""{password}"",
            ""role"": {(int)role}
        }}";

        var registerContent = new StringContent(registerJson, Encoding.UTF8, "application/json");

        var registerResponse = await HttpClient.PostAsync("/api/auth/register", registerContent);

        if (!registerResponse.IsSuccessStatusCode)
        {
            var errorContent = await registerResponse.Content.ReadAsStringAsync();
            throw new Exception(
                $"Registration failed: {registerResponse.StatusCode} - {errorContent}"
            );
        }

        var registerResponseContent = await registerResponse.Content.ReadAsStringAsync();
        var registerResult = JsonSerializer.Deserialize<JsonElement>(registerResponseContent);

        return registerResult.GetProperty("token").GetString()!;
    }

    protected void SetAuthenticationToken(string token)
    {
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );
    }

    protected async Task<HttpResponseMessage> PostAsync<T>(string url, T data)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(
                data,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            ),
            Encoding.UTF8,
            "application/json"
        );
        return await HttpClient.PostAsync(url, content);
    }

    protected async Task<HttpResponseMessage> PutAsync<T>(string url, T data)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(
                data,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            ),
            Encoding.UTF8,
            "application/json"
        );
        return await HttpClient.PutAsync(url, content);
    }

    protected async Task<TResponse?> DeserializeResponse<TResponse>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(
            content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
    }
}
