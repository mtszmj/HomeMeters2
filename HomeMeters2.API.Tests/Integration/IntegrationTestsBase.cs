using System.Text.Json;
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
                    
                    services.AddDbContext<ApplicationDbContext>(options => options
                        .UseInMemoryDatabase("_")//Guid.NewGuid().ToString())
                        .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
                });
            });
        Client = _factory.CreateClient();
    }

    [TearDown]
    public virtual void TearDown()
    {
        _factory.Dispose();
        Client.Dispose();
    }
}