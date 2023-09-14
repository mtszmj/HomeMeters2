namespace HomeMeters2.API.Places.Dtos;

public record PlaceDeletedDto : PlaceDto
{
    public DateTime DateSoftDeletedUtc { get; init; }
}