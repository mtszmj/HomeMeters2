using HomeMeters2.API.Places.Dtos;

namespace HomeMeters2.API.Tests.Integration.Places._PlaceController;

public class GetDeleted : PlaceControllerTestsBase
{
    
    [Test]
    public async Task get_deleted_returns_unauthorized()
    {
        var response = await UnauthorizedClient.GetAsync(DeletedEndpointUri);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Test]
    public async Task get_deleted_returns_ok_for_authorized_client()
    {
        var response = await LoggedInClient1.GetAsync(DeletedEndpointUri);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Test]
    public async Task get_deleted_returns_only_deleted()
    {
        // arrange
        var id1 = (await PostCreatePlace("Test1", $"{nameof(get_deleted_returns_only_deleted)}_1")).Id;
        var id2 = (await PostCreatePlace("Test2", $"{nameof(get_deleted_returns_only_deleted)}_2")).Id;
        var id3 = (await PostCreatePlace("Test3", $"{nameof(get_deleted_returns_only_deleted)}_3")).Id;
        
        _ = await UnauthorizedClient.DeleteAsync($"{EndpointUri}/{id2}");
        _ = await UnauthorizedClient.DeleteAsync($"{EndpointUri}/{id3}");

        // act
        var response = await LoggedInClient1.GetAsync($"{DeletedEndpointUri}");

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
    public async Task get_deleted_returns_only_deleted_owned()
    {
        // arrange
        var id1 = (await PostCreatePlace("Test1", $"{nameof(get_deleted_returns_only_deleted_owned)}_1", LoggedInClient1)).Id;
        var id2 = (await PostCreatePlace("Test2", $"{nameof(get_deleted_returns_only_deleted_owned)}_2", LoggedInClient2)).Id;
        var id3 = (await PostCreatePlace("Test3", $"{nameof(get_deleted_returns_only_deleted_owned)}_3", LoggedInClient1)).Id;
        var id4 = (await PostCreatePlace("Test4", $"{nameof(get_deleted_returns_only_deleted_owned)}_4", LoggedInClient2)).Id;
        var id5 = (await PostCreatePlace("Test5", $"{nameof(get_deleted_returns_only_deleted_owned)}_5", LoggedInClient1)).Id;
        var id6 = (await PostCreatePlace("Test6", $"{nameof(get_deleted_returns_only_deleted_owned)}_6", LoggedInClient2)).Id;
        var id7 = (await PostCreatePlace("Test7", $"{nameof(get_deleted_returns_only_deleted_owned)}_7", LoggedInClient1)).Id;
        
        _ = await LoggedInClient2.DeleteAsync($"{EndpointUri}/{id2}");
        _ = await LoggedInClient1.DeleteAsync($"{EndpointUri}/{id3}");
        _ = await LoggedInClient2.DeleteAsync($"{EndpointUri}/{id4}");
        _ = await LoggedInClient1.DeleteAsync($"{EndpointUri}/{id5}");

        // act
        var response = await LoggedInClient1.GetAsync($"{DeletedEndpointUri}");

        // assert 
        var content = await response.Content.ReadAsStringAsync();
        var places = JsonSerializer.Deserialize<PlaceDeletedDto[]>(content, JsonSerializerOptions);
        places.Should().NotBeNull();
        places.Should().HaveCount(2);
        places[0].Should().NotBeNull();
        places[0].Name.Should().BeOneOf("Test3", "Test5");
        places[1].Should().NotBeNull();
        places[1].Name.Should().BeOneOf("Test3", "Test5");
        places[0].Name.Should().NotBe(places[1].Name);
    }
}