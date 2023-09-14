namespace HomeMeters2.API.Places.Dtos;

public record PlaceDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public DateTime DateCreatedUtc { get; init; }
    public DateTime? DateModifiedUtc { get; init; }
}