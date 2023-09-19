using HomeMeters2.API.Places.Dtos;

namespace HomeMeters2.API.Tests.Integration.Places._PlaceController;

public class Get : PlaceControllerTestsBase
{
    [Test]
    public async Task get_returns_unauthorized()
    {
        var response = await UnauthorizedClient.GetAsync(EndpointUri);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Test]
    public async Task get_returns_ok_for_authorized_client()
    {
        var response = await LoggedInClient1.GetAsync(EndpointUri);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task get_returns_two_places()
    {
        // arrange
        _ = await PostCreatePlace("Test1", $"{nameof(get_returns_two_places)}_1");
        _ = await PostCreatePlace("Test2", $"{nameof(get_returns_two_places)}_2");

        // act
        var response = await LoggedInClient1.GetAsync(EndpointUri);
        
        // assert 
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var placesContent = await response.Content.ReadAsStringAsync();
        var placesDto = JsonSerializer.Deserialize<IEnumerable<PlaceDto>>(placesContent, JsonSerializerOptions)?.ToArray();
        placesDto.Should().NotBeNull();
        placesDto.Should().HaveCount(2);
    }

    [Test]
    public async Task get_returns_only_places_owned()
    {
        // arrange
        _ = await PostCreatePlace("Test1", $"{nameof(get_returns_only_places_owned)}_1", LoggedInClient1);
        _ = await PostCreatePlace("Test2", $"{nameof(get_returns_only_places_owned)}_2", LoggedInClient2);
        _ = await PostCreatePlace("Test3", $"{nameof(get_returns_only_places_owned)}_3", LoggedInClient1);
        _ = await PostCreatePlace("Test4", $"{nameof(get_returns_only_places_owned)}_4", LoggedInClient2);
        _ = await PostCreatePlace("Test5", $"{nameof(get_returns_only_places_owned)}_5", LoggedInClient1);

        // act
        var response = await LoggedInClient1.GetAsync(EndpointUri);
        
        // assert 
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var placesContent = await response.Content.ReadAsStringAsync();
        var placesDto = JsonSerializer.Deserialize<IEnumerable<PlaceDto>>(placesContent, JsonSerializerOptions)?.ToArray();
        placesDto.Should().NotBeNull();
        placesDto.Should().HaveCount(3);
        placesDto.Should().Contain(x => x.Name == "Test1");
        placesDto.Should().Contain(x => x.Name == "Test3");
        placesDto.Should().Contain(x => x.Name == "Test5");
    }
    
    [Test]
    public async Task get_returns_owned_places_not_deleted()
    {
        // arrange
        var p1 = await PostCreatePlace("Test1", $"{nameof(get_returns_owned_places_not_deleted)}_1", LoggedInClient1);
        var p2 = await PostCreatePlace("Test2", $"{nameof(get_returns_owned_places_not_deleted)}_2", LoggedInClient2);
        var p3 = await PostCreatePlace("Test3", $"{nameof(get_returns_owned_places_not_deleted)}_3", LoggedInClient1);
        var p4 = await PostCreatePlace("Test4", $"{nameof(get_returns_owned_places_not_deleted)}_4", LoggedInClient2);
        var p5 = await PostCreatePlace("Test4", $"{nameof(get_returns_owned_places_not_deleted)}_5", LoggedInClient1);
        var p6 = await PostCreatePlace("Test4", $"{nameof(get_returns_owned_places_not_deleted)}_6", LoggedInClient2);
        var p7 = await PostCreatePlace("Test4", $"{nameof(get_returns_owned_places_not_deleted)}_7", LoggedInClient1);

        await LoggedInClient2.DeleteAsync($"{EndpointUri}/{p2.Id}");
        await LoggedInClient1.DeleteAsync($"{EndpointUri}/{p3.Id}");
        await LoggedInClient2.DeleteAsync($"{EndpointUri}/{p4.Id}");
        await LoggedInClient1.DeleteAsync($"{EndpointUri}/{p5.Id}");

        // act
        var response = await LoggedInClient1.GetAsync(EndpointUri);
        
        // assert 
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var placesContent = await response.Content.ReadAsStringAsync();
        var placesDto = JsonSerializer.Deserialize<PlaceDto[]>(placesContent, JsonSerializerOptions);
        placesDto.Should().NotBeNull();
        placesDto.Should().HaveCount(2);
        placesDto.Select(x => x.Id).Should().BeEquivalentTo(new[] { p1.Id, p7.Id });
    }

}