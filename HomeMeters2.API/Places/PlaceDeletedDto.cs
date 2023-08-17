namespace HomeMeters2.API.Places;

public class PlaceDeletedDto : PlaceDto
{
    public DateTime DateSoftDeletedUtc { get; set; }
}