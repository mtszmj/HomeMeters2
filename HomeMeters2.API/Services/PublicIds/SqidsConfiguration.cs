using System.ComponentModel.DataAnnotations;

namespace HomeMeters2.API.Services.PublicIds;

public class SqidsConfiguration
{
    public required string Alphabet { get; set; }
    
    [Range(6,20)]
    public required int MinimumLength { get; set; }
}