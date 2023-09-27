using System.Net.Http.Json;
using HomeMeters2.API.Places.Dtos;

namespace HomeMeters2.API.Tests.Integration.Places;

public class PlaceControllerTests : IntegrationTestsBase
{
    private const string EndpointUri = "/api/place";
    private const string DeletedEndpointUri = "/api/place/deleted";
    
    
    private async Task<PlaceDto> PostCreatePlace(string name, string description)
    {
        var dto = new CreatePlaceDto
        {
            Name = name,
            Description = description
        };
        var content = JsonContent.Create(dto);
        var createResponse = await LoggedInClient1.PostAsync(EndpointUri, content);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var placeDto = JsonSerializer.Deserialize<PlaceDto>(createResponseContent, JsonSerializerOptions);
        return placeDto ?? new PlaceDto { Id = "", Name = "", Description = "" };
    }

    [Test]
    public async Task create_returns_created()
    {
        // arrange
        var dto = new CreatePlaceDto
        {
            Name = "Test",
            Description = nameof(create_returns_created)
        };
        var content = JsonContent.Create(dto);
        
        // act
        var response = await LoggedInClient1.PostAsync(EndpointUri, content);
        
        // assert 
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Should().ContainKey("Location");
        
        var createdContent = await response.Content.ReadAsStringAsync();
        var placeDto = JsonSerializer.Deserialize<PlaceDto>(createdContent, JsonSerializerOptions);
        placeDto.Should().NotBeNull();
        placeDto.Name.Should().Be("Test");
        placeDto.Description.Should().Be(nameof(create_returns_created));
    }

    [Test]
    public async Task delete_returns_no_content()
    {
        // arrange
        var id1 = (await PostCreatePlace("Test1", $"{nameof(delete_returns_no_content)}_1")).Id;
        var id2 = (await PostCreatePlace("Test2", $"{nameof(delete_returns_no_content)}_2")).Id;
        var id3 = (await PostCreatePlace("Test3", $"{nameof(delete_returns_no_content)}_3")).Id;
        
        // act
        var response = await UnauthorizedClient.DeleteAsync($"{EndpointUri}/{id2}");
        
        // assert 
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [Test]
    public async Task update_returns_no_content()
    {
        // arrange
        var created = (await PostCreatePlace("Test1", $"{nameof(update_returns_no_content)}_1"));
        

        // act
        var dto = new UpdatePlaceDto(created.Id, "Test1_updated", $"{nameof(update_returns_no_content)}_1_updated");
        var updateContent = JsonContent.Create(dto);
        var updateResponse = await UnauthorizedClient.PutAsync(EndpointUri, updateContent);

        // assert 
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [Test]
    public async Task update_changes_place_data()
    {
        // arrange
        var created = (await PostCreatePlace("Test1", $"{nameof(update_changes_place_data)}_1"));

        // act
        var dto = new UpdatePlaceDto(created.Id, "Test1_updated", $"{nameof(update_changes_place_data)}_1_updated");
        var updateContent = JsonContent.Create(dto);
        _ = await UnauthorizedClient.PutAsync(EndpointUri, updateContent);

        // assert 
        var response = await LoggedInClient1.GetAsync($"{EndpointUri}/{created.Id}");
        var placesContent = await response.Content.ReadAsStringAsync();
        var placesDto = JsonSerializer.Deserialize<PlaceDto>(placesContent, JsonSerializerOptions);

        placesDto.Name.Should().Be("Test1_updated");
        placesDto.Description.Should().Be($"{nameof(update_changes_place_data)}_1_updated");
        placesDto.DateCreatedUtc.Should().Be(created.DateCreatedUtc);
        placesDto.DateModifiedUtc.Should().NotBeNull();
    }
    
    [Test]
    public async Task update_returns_not_found()
    {
        // arrange

        // act
        var dto = new UpdatePlaceDto("notfoundid", "Test1_updated", $"{nameof(update_returns_not_found)}_1_updated");
        var updateContent = JsonContent.Create(dto);
        var response = await UnauthorizedClient.PutAsync(EndpointUri, updateContent);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}