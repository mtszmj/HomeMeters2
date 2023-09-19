using System.Net.Http.Json;
using HomeMeters2.API.Places.Dtos;

namespace HomeMeters2.API.Tests.Integration.Places._PlaceController;

public class PlaceControllerTestsBase : IntegrationTestsBase
{
    protected string EndpointUri { get; } = "/api/place";

    protected async Task<PlaceDto> PostCreatePlace(string name, string description, HttpClient? client = null)
    {
        client ??= LoggedInClient1;
        var dto = new CreatePlaceDto
        {
            Name = name,
            Description = description
        };
        var content = JsonContent.Create(dto);
        var createResponse = await client.PostAsync(EndpointUri, content);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var placeDto = JsonSerializer.Deserialize<PlaceDto>(createResponseContent, JsonSerializerOptions);
        return placeDto ?? new PlaceDto { Id = "", Name = "", Description = "" };
    }
}