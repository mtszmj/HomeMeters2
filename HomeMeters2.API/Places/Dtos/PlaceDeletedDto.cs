namespace HomeMeters2.API.Places.Dtos;

public class PlaceDeletedDto : PlaceDto
{
    public DateTime DateSoftDeletedUtc { get; set; }
}