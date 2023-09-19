namespace HomeMeters2.API.Tests.Integration.HealthChecks;

public class HealthChecksEndpointTests : IntegrationTestsBase
{
    [Test]
    public async Task health_check_endpoint_returns_ok_and_healthy()
    {
        var response = await UnauthorizedClient.GetAsync("/api/healthcheck");
        var healthCheckStatus = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        healthCheckStatus.Should().BeEquivalentTo("Healthy");
    }
}