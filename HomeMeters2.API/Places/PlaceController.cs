using Microsoft.AspNetCore.Mvc;

namespace HomeMeters2.API.Places;

[ApiController]
[Route("api/[controller]")]
public class PlaceController : ControllerBase
{
    private readonly InMemoryTestData _repository;

    public PlaceController(InMemoryTestData repository)
    {
        _repository = repository;
    }
    
    [HttpGet()]
    public IEnumerable<PlaceDto> GetPlaces()
    {
        return _repository.Places.Where(x => !x.IsSoftDeleted).Select(x => new PlaceDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            DateCreatedUtc = x.DateCreatedUtc,
            DateModifiedUtc = x.DateModifiedUtc
        });
    }

    [HttpGet("Deleted")]
    public IEnumerable<PlaceDeletedDto> GetDeletedPlaces()
    {
        return _repository.Places.Where(x => x.IsSoftDeleted).Select(x => new PlaceDeletedDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            DateCreatedUtc = x.DateCreatedUtc,
            DateModifiedUtc = x.DateModifiedUtc,
            DateSoftDeletedUtc = x.DateSoftDeletedUtc!.Value
        });
    } 

    [HttpGet("{id:int}")]
    public ActionResult<PlaceDto> GetPlace(int id)
    {
        var place = _repository.Places.FirstOrDefault(x => x.Id == id);
        if (place is null)
            return NotFound();
        
        return new PlaceDto
        {
            Id = place.Id,
            Name = place.Name,
            Description = place.Description,
            DateCreatedUtc = place.DateCreatedUtc,
            DateModifiedUtc = place.DateModifiedUtc
        };
    }

    [HttpPost]
    public ActionResult<string> CreatePlace(CreatePlaceDto dto)
    {
        var id = _repository.Places.Max(x => x.Id) + 1;
        var place = new Place(dto.Name, dto.Description ?? string.Empty, TimeProvider.System.GetUtcNow().UtcDateTime)
        {
            Id = id
        };
        
        _repository.Places.Add(place);
        return CreatedAtAction(nameof(GetPlace), new { id = place.Id }, new PlaceDto
        {
            Id = place.Id,
            Name = place.Name,
            Description = place.Description,
            DateCreatedUtc = place.DateCreatedUtc,
            DateModifiedUtc = place.DateModifiedUtc
        });
    }

    [HttpPut]
    public ActionResult UpdatePlace(UpdatePlaceDto dto)
    {
        var place = _repository.Places.FirstOrDefault(x => x.Id == dto.Id);
        if (place is null)
            return NotFound();

        var result = place.Update(dto.Name, dto.Description);
        
        return result ? NoContent() : BadRequest("No update took place");
    }

    [HttpDelete("{id}")]
    public ActionResult DeletePlace(int id)
    {
        var place = _repository.Places.FirstOrDefault(x => x.Id == id);
        if (place is null)
            return NotFound();

        place.Delete();
        return NoContent();
    }
    
}