using System.Net.Http.Json;
using System.Text.Json;
using HomeMeters2.API.Places.Dtos;

namespace HomeMeters2.API.Tests.Integration.Places;

public class PlaceControllerTests : IntegrationTestsBase
{
    private const string EndpointUri = "/api/place";
    private const string DeletedEndpointUri = "/api/place/deleted";
    
    [Test]
    public async Task get_returns_ok()
    {
        var response = await Client.GetAsync(EndpointUri);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task get_returns_two_places()
    {
        // arrange
        _ = await PostCreatePlace("Test1", $"{nameof(get_returns_two_places)}_1");
        _ = await PostCreatePlace("Test2", $"{nameof(get_returns_two_places)}_2");

        // act
        var response = await Client.GetAsync(EndpointUri);
        
        // assert 
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var placesContent = await response.Content.ReadAsStringAsync();
        var placesDto = JsonSerializer.Deserialize<IEnumerable<PlaceDto>>(placesContent, JsonSerializerOptions);
        placesDto.Should().NotBeNull();
        placesDto.Should().HaveCount(2);
    }

    [Test]
    public async Task get_returns_places_not_deleted()
    {
        // arrange
        var p1 = await PostCreatePlace("Test1", $"{nameof(get_returns_two_places)}_1");
        var p2 = await PostCreatePlace("Test2", $"{nameof(get_returns_two_places)}_2");
        var p3 = await PostCreatePlace("Test3", $"{nameof(get_returns_two_places)}_3");
        var p4 = await PostCreatePlace("Test4", $"{nameof(get_returns_two_places)}_4");

        await Client.DeleteAsync($"{EndpointUri}/{p2.Id}");
        await Client.DeleteAsync($"{EndpointUri}/{p4.Id}");

        // act
        var response = await Client.GetAsync(EndpointUri);
        
        // assert 
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var placesContent = await response.Content.ReadAsStringAsync();
        var placesDto = JsonSerializer.Deserialize<PlaceDto[]>(placesContent, JsonSerializerOptions);
        placesDto.Should().NotBeNull();
        placesDto.Should().HaveCount(2);
        placesDto.Select(x => x.Id).Should().BeEquivalentTo(new[] { 1, 3 });
    }

    [Test]
    public async Task get_id_returns_single_place()
    {
        // arrange
        var id = (await PostCreatePlace("Test1", $"{nameof(get_id_returns_single_place)}_1")).Id;
        _ = await PostCreatePlace("Test2", $"{nameof(get_id_returns_single_place)}_2");
        
        // act
        var response = await Client.GetAsync($"{EndpointUri}/{id}");
        
        // assert 
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var placesContent = await response.Content.ReadAsStringAsync();
        var placesDto = JsonSerializer.Deserialize<PlaceDto>(placesContent, JsonSerializerOptions);
        placesDto.Should().NotBeNull();
        placesDto.Id.Should().Be(id);
        placesDto.Description.Should().Be($"{nameof(get_id_returns_single_place)}_1");
    }

    private async Task<PlaceDto> PostCreatePlace(string name, string description)
    {
        var dto = new CreatePlaceDto
        {
            Name = name,
            Description = description
        };
        var content = JsonContent.Create(dto);
        var createResponse = await Client.PostAsync(EndpointUri, content);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var placeDto = JsonSerializer.Deserialize<PlaceDto>(createResponseContent, JsonSerializerOptions);
        return placeDto ?? new PlaceDto { Id = -1, Name = "", Description = "" };
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
        var response = await Client.PostAsync(EndpointUri, content);
        
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
        var response = await Client.DeleteAsync($"{EndpointUri}/{id2}");
        
        // assert 
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task get_deleted_id_returns_single_deleted_place()
    {
        // arrange
        var id1 = (await PostCreatePlace("Test1", $"{nameof(get_deleted_id_returns_single_deleted_place)}_1")).Id;
        var id2 = (await PostCreatePlace("Test2", $"{nameof(get_deleted_id_returns_single_deleted_place)}_2")).Id;
        var id3 = (await PostCreatePlace("Test3", $"{nameof(get_deleted_id_returns_single_deleted_place)}_3")).Id;
        
        _ = await Client.DeleteAsync($"{EndpointUri}/{id2}");
        _ = await Client.DeleteAsync($"{EndpointUri}/{id3}");

        // act
        var response = await Client.GetAsync($"{DeletedEndpointUri}/{id2}");

        // assert 
        var content = await response.Content.ReadAsStringAsync();
        var placeDto = JsonSerializer.Deserialize<PlaceDeletedDto>(content, JsonSerializerOptions);
        placeDto.Should().NotBeNull();
        placeDto.Name.Should().Be("Test2");
        placeDto.Description.Should().Be($"{nameof(get_deleted_id_returns_single_deleted_place)}_2");
    }

    [Test]
    public async Task get_deleted_returns_only_deleted()
    {
        // arrange
        var id1 = (await PostCreatePlace("Test1", $"{nameof(get_deleted_id_returns_single_deleted_place)}_1")).Id;
        var id2 = (await PostCreatePlace("Test2", $"{nameof(get_deleted_id_returns_single_deleted_place)}_2")).Id;
        var id3 = (await PostCreatePlace("Test3", $"{nameof(get_deleted_id_returns_single_deleted_place)}_3")).Id;
        
        _ = await Client.DeleteAsync($"{EndpointUri}/{id2}");
        _ = await Client.DeleteAsync($"{EndpointUri}/{id3}");

        // act
        var response = await Client.GetAsync($"{DeletedEndpointUri}");

        // assert 
        var content = await response.Content.ReadAsStringAsync();
        var places = JsonSerializer.Deserialize<PlaceDeletedDto[]>(content, JsonSerializerOptions);
        places.Should().NotBeNull();
        places.Should().HaveCount(2);
        places[0].Should().NotBeNull();
        places[0].Name.Should().BeOneOf("Test2", "Test3");
        places[1].Should().NotBeNull();
        places[1].Name.Should().BeOneOf("Test2", "Test3");
        places[0].Name.Should().NotBe(places[1].Name);
    }

    [Test]
    public async Task update_returns_no_content()
    {
        // arrange
        var created = (await PostCreatePlace("Test1", $"{nameof(update_returns_no_content)}_1"));
        

        // act
        var dto = new UpdatePlaceDto(created.Id, "Test1_updated", $"{nameof(update_returns_no_content)}_1_updated");
        var updateContent = JsonContent.Create(dto);
        var updateResponse = await Client.PutAsync(EndpointUri, updateContent);

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
        _ = await Client.PutAsync(EndpointUri, updateContent);

        // assert 
        var response = await Client.GetAsync($"{EndpointUri}/{created.Id}");
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
        var dto = new UpdatePlaceDto(1, "Test1_updated", $"{nameof(update_returns_not_found)}_1_updated");
        var updateContent = JsonContent.Create(dto);
        var response = await Client.PutAsync(EndpointUri, updateContent);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}