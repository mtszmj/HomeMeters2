using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HomeMeters2.API.Constants;
using HomeMeters2.API.DataAccess;
using HomeMeters2.API.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace HomeMeters2.API.Tests.Integration;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class IntegrationTestsBase
{
    private WebApplicationFactory<Program> _factory;
    private Lazy<HttpClient> LazyUnauthorizedClient;
    private Lazy<HttpClient> LazyLoggedInClient1;
    private Lazy<HttpClient> LazyLoggedInClient2;
    protected HttpClient UnauthorizedClient => LazyUnauthorizedClient.Value;
    protected HttpClient LoggedInClient1 => LazyLoggedInClient1.Value;
    protected HttpClient LoggedInClient2 => LazyLoggedInClient2.Value;

    protected JsonSerializerOptions JsonSerializerOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [SetUp]
    public virtual void SetUp()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.FlagAsIntegrationTest();
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                         typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null) services.Remove(descriptor);

                var inMemoryDbId = Guid.NewGuid().ToString();
                services.AddDbContext<ApplicationDbContext>(options => options
                    .UseInMemoryDatabase(inMemoryDbId)
                    .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
            });
        });
        LazyUnauthorizedClient = new Lazy<HttpClient>(() => _factory.CreateClient());
        LazyLoggedInClient1 = new Lazy<HttpClient>(() => CreateLoggedInClient("test@test.test1"));
        LazyLoggedInClient2 = new Lazy<HttpClient>(() => CreateLoggedInClient("test@test.test2"));
    }

    [TearDown]
    public virtual void TearDown()
    {
        _factory.Dispose();
        UnauthorizedClient.Dispose();
        if (LazyLoggedInClient1.IsValueCreated)
            LazyLoggedInClient1.Value.Dispose();
    }

    private HttpClient CreateLoggedInClient(string email)
    {
        var httpClient = _factory.CreateClient();
        var content = JsonContent.Create(new
        {
            Email = email,
            Password = "Test1!"
        });
        var result = httpClient.PostAsync($"{UsersConstants.EndpointPath}/register", content).GetAwaiter().GetResult();

        content = JsonContent.Create(new
        {
            Email = email,
            Password = "Test1!"
        });
        var result2 = httpClient.PostAsync($"{UsersConstants.EndpointPath}/login", content).GetAwaiter().GetResult();
        var loginResultContent = result2.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        var loginResult = JsonSerializer.Deserialize<LoginResult>(loginResultContent, JsonSerializerOptions);

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.AccessToken);
        return httpClient;
    }

    private class LoginResult
    {
        public string TokenType { get; set; }
        public string AccessToken { get; set; }
        public long ExpiresIn { get; set; }
        public string RefreshToken { get; set; }
    }
}