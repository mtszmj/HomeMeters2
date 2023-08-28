using Microsoft.AspNetCore.Mvc.Testing;
using HomeMeters2.API.Extensions;

namespace HomeMeters2.API.Tests.Integration;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class IntegrationTestsBase
{
    private readonly WebApplicationFactory<Program> _factory;
    protected readonly HttpClient Client;

    public IntegrationTestsBase()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.FlagAsIntegrationTest());
        Client = _factory.CreateClient();
    }

    [TearDown]
    public virtual void TearDown()
    {
        _factory.Dispose();
        Client.Dispose();
    }
}