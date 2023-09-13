using HomeMeters2.API.DataAccess;
using HomeMeters2.API.Logging;
using HomeMeters2.API.Places.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeMeters2.API.Places;

[ApiController]
[Route("api/[controller]")]
public class PlaceController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PlaceController> _logger;

    public PlaceController(ApplicationDbContext dbContext, ILogger<PlaceController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PlaceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PlaceDto>>> GetPlaces()
    {
        try
        {
            return Ok(await _dbContext.Places.ToListAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(LogIds.Place.ControllerException, ex, "CreatePlace exception: {Message}", ex.Message);
            return Problem();
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PlaceDeletedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlaceDto>> GetPlace(int id)
    {
        try
        {
            var place = await _dbContext.Places.FirstOrDefaultAsync(x => x.Id == id);
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
        catch (Exception ex)
        {
            _logger.LogError(LogIds.Place.ControllerException, ex, "CreatePlace exception: {Message}", ex.Message);
            return Problem();
        }
    }

    [HttpGet("Deleted")]
    [ProducesResponseType(typeof(IEnumerable<PlaceDeletedDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PlaceDeletedDto>>> GetDeletedPlaces()
    {
        try
        {
            return Ok(await _dbContext.Places.IgnoreQueryFilters().Where(x => x.IsSoftDeleted).Select(x =>
                new PlaceDeletedDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    DateCreatedUtc = x.DateCreatedUtc,
                    DateModifiedUtc = x.DateModifiedUtc,
                    DateSoftDeletedUtc = x.DateSoftDeletedUtc!.Value
                }).ToListAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(LogIds.Place.ControllerException, ex, "CreatePlace exception: {Message}", ex.Message);
            return Problem();
        }
    }

    [HttpGet("Deleted/{id:int}")]
    [ProducesResponseType(typeof(PlaceDeletedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlaceDeletedDto>> GetDeletedPlace(int id)
    {
        try
        {
            var place = await _dbContext.Places.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsSoftDeleted);
            if (place is null)
                return NotFound();

            return new PlaceDeletedDto
            {
                Id = place.Id,
                Name = place.Name,
                Description = place.Description,
                DateCreatedUtc = place.DateCreatedUtc,
                DateModifiedUtc = place.DateModifiedUtc,
                DateSoftDeletedUtc = place.DateSoftDeletedUtc ??
                                     DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(LogIds.Place.ControllerException, ex, "CreatePlace exception: {Message}", ex.Message);
            return Problem();
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(PlaceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> CreatePlace(CreatePlaceDto dto)
    {
        try
        {
            var place = new Place(dto.Name, dto.Description ?? string.Empty,
                TimeProvider.System.GetUtcNow().UtcDateTime);

            _dbContext.Places.Add(place);
            var count = await _dbContext.SaveChangesAsync();

            if (count == 0) return BadRequest("Could not save place.");

            return CreatedAtAction(nameof(GetPlace), new { id = place.Id }, new PlaceDto
            {
                Id = place.Id,
                Name = place.Name,
                Description = place.Description,
                DateCreatedUtc = place.DateCreatedUtc,
                DateModifiedUtc = place.DateModifiedUtc
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(LogIds.Place.ControllerException, ex, "CreatePlace exception: {Message}", ex.Message);
            return Problem();
        }
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdatePlace(UpdatePlaceDto dto)
    {
        try
        {
            var place = await _dbContext.Places.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (place is null)
                return NotFound();

            var result = place.Update(dto.Name, dto.Description);
            if (!result)
                return BadRequest("No update took place");

            var updated = await _dbContext.SaveChangesAsync();

            return updated > 0 ? NoContent() : BadRequest("No update took place");
        }
        catch (Exception ex)
        {
            _logger.LogError(LogIds.Place.ControllerException, ex, "CreatePlace exception: {Message}", ex.Message);
            return Problem();
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeletePlace(int id)
    {
        try
        {
            var place = await _dbContext.Places.FirstOrDefaultAsync(x => x.Id == id);
            if (place is null)
                return NotFound();

            place.Delete();
            var updated = await _dbContext.SaveChangesAsync();
            return updated > 0 ? NoContent() : BadRequest("No delete took place");
        }
        catch (Exception ex)
        {
            _logger.LogError(LogIds.Place.ControllerException, ex, "CreatePlace exception: {Message}", ex.Message);
            return Problem();
        }
    }
}