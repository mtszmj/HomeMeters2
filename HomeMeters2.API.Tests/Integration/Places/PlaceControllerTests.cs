using System.Net.Http.Json;
using System.Text.Json;
using HomeMeters2.API.Places;
using HomeMeters2.API.Places.Dtos;

namespace HomeMeters2.API.Tests.Integration.Places;

public class PlaceControllerTests : IntegrationTestsBase
{
    [Test]
    public async Task place_endpoint_get_returns_ok()
    {
        var response = await Client.GetAsync("/api/place");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Test]
    public async Task place_endpoint_create_returns_created()
    {
        // arrange
        var dto = new CreatePlaceDto
        {
            Name = "Test",
            Description = "TestDescription"
        };
        var content = JsonContent.Create(dto);
        
        // act
        var response = await Client.PostAsync("/api/place", content);
        
        // assert 
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Should().ContainKey("Location");
        
        var createdContent = await response.Content.ReadAsStringAsync();
        var placeDto = JsonSerializer.Deserialize<PlaceDto>(createdContent, JsonSerializerOptions);
        placeDto.Should().NotBeNull();
        placeDto.Name.Should().Be("Test");
        placeDto.Description.Should().Be("TestDescription");
    }
}