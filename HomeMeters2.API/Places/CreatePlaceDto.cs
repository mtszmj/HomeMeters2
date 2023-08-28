namespace HomeMeters2.API.Places;

public class CreatePlaceDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}