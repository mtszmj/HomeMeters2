using System.ComponentModel.DataAnnotations;
using HomeMeters2.API.Constants;

namespace HomeMeters2.API.Places.Dtos;

public class CreatePlaceDto
{
    [StringLength(PlaceConstants.PlaceNameMaximumLength, MinimumLength = PlaceConstants.PlaceNameMinimumLength)]
    public required string Name { get; set; }
    
    
    [StringLength(PlaceConstants.PlaceDescriptionMaximumLength)]
    public string? Description { get; set; }
}