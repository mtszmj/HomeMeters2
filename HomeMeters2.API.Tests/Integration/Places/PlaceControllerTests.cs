using System.Net;
using HomeMeters2.API.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HomeMeters2.API.Tests.Integration.Places;

[TestFixture]
public class PlaceControllerTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    
    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.FlagAsIntegrationTest());
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _factory.Dispose();
        _client.Dispose();
    }

    [Test]
    public async Task TestGetEndpoint()
    {
        var response = await _client.GetAsync("/api/place");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}