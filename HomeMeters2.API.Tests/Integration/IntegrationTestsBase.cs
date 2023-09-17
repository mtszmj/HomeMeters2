using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HomeMeters2.API.Constants;
using HomeMeters2.API.DataAccess;
using Microsoft.AspNetCore.Mvc.Testing;
using HomeMeters2.API.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace HomeMeters2.API.Tests.Integration;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class IntegrationTestsBase
{
    private readonly WebApplicationFactory<Program> _factory;
    protected readonly HttpClient Client;
    protected HttpClient LoggedInClient => LazyLoggedInClient.Value;
    private readonly Lazy<HttpClient> LazyLoggedInClient;

    protected JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public IntegrationTestsBase()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.FlagAsIntegrationTest();
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType ==
                             typeof(DbContextOptions<ApplicationDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    var inMemoryDbId = Guid.NewGuid().ToString();
                    services.AddDbContext<ApplicationDbContext>(options => options
                        .UseInMemoryDatabase(inMemoryDbId)
                        .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
                });
            });
        Client = _factory.CreateClient();
        LazyLoggedInClient = new Lazy<HttpClient>(CreateLoggedInClient);
    }

    private HttpClient CreateLoggedInClient()
    {
        var httpClient = _factory.CreateClient();
        var content = JsonContent.Create(new 
        {
            Username = "inttest",
            Password = "Test1!",
            Email = "test@test.test1"
        });
        var result = httpClient.PostAsync($"{UsersConstants.EndpointPath}/register", content).GetAwaiter().GetResult();

        content = JsonContent.Create(new
        {
            Username = "inttest",
            Password = "Test1!",
        });
        var result2 = httpClient.PostAsync($"{UsersConstants.EndpointPath}/login", content).GetAwaiter().GetResult();
        var loginResultContent = result2.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        
        var loginResult = JsonSerializer.Deserialize<LoginResult>(loginResultContent, JsonSerializerOptions);

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.access_token);
        return httpClient;
    }
    
    private class LoginResult
    {
        public string token_type { get; set; }
        public string access_token { get; set; }
        public long expires_in { get; set; }
        public string refresh_token { get; set; }
    }

    [TearDown]
    public virtual void TearDown()
    {
        _factory.Dispose();
        Client.Dispose();
    }
}