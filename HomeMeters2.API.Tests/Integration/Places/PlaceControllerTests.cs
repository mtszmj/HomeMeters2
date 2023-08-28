namespace HomeMeters2.API.Tests.Integration.Places;

public class PlaceControllerTests : IntegrationTestsBase
{
    [Test]
    public async Task place_endpoint_get_returns_ok()
    {
        var response = await Client.GetAsync("/api/place");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}