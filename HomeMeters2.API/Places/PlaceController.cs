using System.Security.Claims;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using HomeMeters2.API.Constants;
using HomeMeters2.API.DataAccess;
using HomeMeters2.API.Logging;
using HomeMeters2.API.Places.Dtos;
using HomeMeters2.API.Services.PublicIds;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeMeters2.API.Places;

[ApiController]
[Route("api/[controller]")]
public class PlaceController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly PublicIdGenerator _publicIdGenerator;
    private readonly IMapper _mapper;
    private readonly ILogger<PlaceController> _logger;

    public PlaceController(ApplicationDbContext dbContext,
        PublicIdGenerator publicIdGenerator,
        IMapper mapper,
        ILogger<PlaceController> logger
        )
    {
        _dbContext = dbContext;
        _publicIdGenerator = publicIdGenerator;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PlaceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PlaceDto>>> GetPlaces()
    {
        try
        {
            var places = await _dbContext.Places.ToListAsync();
            var dtos = places.Select(x => _mapper.Map<PlaceDto>(x) with { Id = ToPublicId(x) });
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(LogIds.Place.ControllerException, ex, "CreatePlace exception: {Message}", ex.Message);
            return Problem();
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PlaceDeletedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlaceDto>> GetPlace(string id)
    {
        try
        {
            var decodedId = PublicToInternalId(id);
            var place = await _dbContext.Places.FirstOrDefaultAsync(x => x.Id == decodedId);
            if (place is null)
                return NotFound();

            return _mapper.Map<PlaceDto>(place) with { Id = ToPublicId(place) };
        }
        catch (Exception ex)
        {
            _logger.LogError(LogIds.Place.ControllerException, ex, "CreatePlace exception: {Message}", ex.Message);
            return Problem();
        }
    }

    [HttpGet("Deleted")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<PlaceDeletedDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PlaceDeletedDto>>> GetDeletedPlaces()
    {
        try
        {
            var user = User;
            var places = await _dbContext.Places.IgnoreQueryFilters().Where(x => x.IsSoftDeleted).ToListAsync();

            var dtos = places.Select(x => _mapper.Map<PlaceDeletedDto>(x) with { Id = ToPublicId(x) });
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(LogIds.Place.ControllerException, ex, "CreatePlace exception: {Message}", ex.Message);
            return Problem();
        }
    }

    [HttpGet("Deleted/{id}")]
    [ProducesResponseType(typeof(PlaceDeletedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlaceDeletedDto>> GetDeletedPlace(string id)
    {
        try
        {
            var decodedId = PublicToInternalId(id);
            var place = await _dbContext.Places.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == decodedId && x.IsSoftDeleted);
            if (place is null)
                return NotFound();

            var dto = _mapper.Map<PlaceDeletedDto>(place) with { Id = ToPublicId(place) };
            return dto;
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

            var publicId = ToPublicId(place);
            return CreatedAtAction(
                nameof(GetPlace), 
                new { id = publicId },
                _mapper.Map<PlaceDto>(place) with { Id = publicId }
            );
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
            var decodedId = PublicToInternalId(dto.Id);
            var place = await _dbContext.Places.FirstOrDefaultAsync(x => x.Id == decodedId);
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
    public async Task<ActionResult> DeletePlace(string id)
    {
        try
        {
            var decodedId = PublicToInternalId(id);
            var place = await _dbContext.Places.FirstOrDefaultAsync(x => x.Id == decodedId);
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

    private int PublicToInternalId(string publicId)
    {
        var decoded = _publicIdGenerator.Decode(publicId);
        return decoded.Count > 1 ? decoded[1] : 0;
    }

    private string ToPublicId(Place place)
    {
        return _publicIdGenerator.Encode(PlaceConstants.PlacePublicIdAppendValue, place.Id);
    }
}