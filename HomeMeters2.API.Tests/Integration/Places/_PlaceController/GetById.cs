using HomeMeters2.API.Places.Dtos;

namespace HomeMeters2.API.Tests.Integration.Places._PlaceController;

public class GetById : PlaceControllerTestsBase
{
    [Test]
    public async Task get_id_returns_unauthorized_when_id_exists()
    {
        // arrange
        var id = (await PostCreatePlace("Test1", $"{nameof(get_id_returns_single_place)}_1", LoggedInClient1)).Id;
        _ = await PostCreatePlace("Test2", $"{nameof(get_id_returns_single_place)}_2", LoggedInClient1);
        
        // act
        var response = await UnauthorizedClient.GetAsync($"{EndpointUri}/{id}");
        
        // assert 
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Test]
    public async Task get_id_returns_unauthorized_when_id_does_not_exist()
    {
        // act
        var response = await UnauthorizedClient.GetAsync($"{EndpointUri}/notexist");
        
        // assert 
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Test]
    public async Task get_id_returns_not_found()
    {
        // arrange
        _ = await PostCreatePlace("Test1", $"{nameof(get_id_returns_single_place)}_1", LoggedInClient1);
        _ = await PostCreatePlace("Test2", $"{nameof(get_id_returns_single_place)}_2", LoggedInClient1);
        
        // act
        var response = await LoggedInClient1.GetAsync($"{EndpointUri}/notexists!@#");
        
        // assert 
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Test]
    public async Task get_id_returns_single_place()
    {
        // arrange
        var id = (await PostCreatePlace("Test1", $"{nameof(get_id_returns_single_place)}_1")).Id;
        _ = await PostCreatePlace("Test2", $"{nameof(get_id_returns_single_place)}_2");
        
        // act
        var response = await LoggedInClient1.GetAsync($"{EndpointUri}/{id}");
        
        // assert 
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var placesContent = await response.Content.ReadAsStringAsync();
        var placesDto = JsonSerializer.Deserialize<PlaceDto>(placesContent, JsonSerializerOptions);
        placesDto.Should().NotBeNull();
        placesDto.Id.Should().Be(id);
        placesDto.Description.Should().Be($"{nameof(get_id_returns_single_place)}_1");
    }
    
    [Test]
    public async Task get_id_returns_not_found_when_id_has_different_owner()
    {
        // arrange
        _ = await PostCreatePlace("Test1", $"{nameof(get_id_returns_single_place)}_1", LoggedInClient1);
        _ = await PostCreatePlace("Test2", $"{nameof(get_id_returns_single_place)}_2", LoggedInClient1);
        var other = (await PostCreatePlace("Test2", $"{nameof(get_id_returns_single_place)}_2", LoggedInClient2)).Id;
        
        // act
        var response = await LoggedInClient1.GetAsync($"{EndpointUri}/{other}");
        
        // assert 
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);       
    }
    
    [Test]
    public async Task get_id_returns_not_found_if_deleted()
    {
        // arrange
        var id = (await PostCreatePlace("Test1", $"{nameof(get_id_returns_single_place)}_1")).Id;
        _ = await PostCreatePlace("Test2", $"{nameof(get_id_returns_single_place)}_2");
        _ = await LoggedInClient1.DeleteAsync($"{EndpointUri}/{id}");
        
        // act
        var response = await LoggedInClient1.GetAsync($"{EndpointUri}/{id}");
        
        // assert 
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Test]
    public async Task get_id_returns_not_found_if_deleted_and_other_owner()
    {
        // arrange
        var id = (await PostCreatePlace("Test1", $"{nameof(get_id_returns_single_place)}_1", LoggedInClient2)).Id;
        _ = await PostCreatePlace("Test2", $"{nameof(get_id_returns_single_place)}_2");
        _ = await LoggedInClient2.DeleteAsync($"{EndpointUri}/{id}");
        
        // act
        var response = await LoggedInClient1.GetAsync($"{EndpointUri}/{id}");
        
        // assert 
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}