using AutoMapper;
using HomeMeters2.API.Constants;
using HomeMeters2.API.DataAccess;
using HomeMeters2.API.Logging;
using HomeMeters2.API.Places.Dtos;
using HomeMeters2.API.Services.PublicIds;
using HomeMeters2.API.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeMeters2.API.Places;

[ApiController]
[Route("api/[controller]")]
public class PlaceController(ApplicationDbContext dbContext,
        PublicIdGenerator publicIdGenerator,
        UserManager<AppUser> userManager,
        IMapper mapper,
        ILogger<PlaceController> logger)
    : ControllerBase
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<PlaceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PlaceDto>>> GetPlaces()
    {
        try
        {
            if (await userManager.GetUserAsync(User) is not { } user) return Unauthorized();

            var places = await dbContext.Places.Where(x => x.OwnerId == user.Id).ToListAsync();
            var dtos = places.Select(x => mapper.Map<PlaceDto>(x) with { Id = ToPublicId(x) });
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(LogIds.Place.ControllerException, ex, "CreatePlace exception: {Message}", ex.Message);
            return Problem();
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(PlaceDeletedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlaceDto>> GetPlace(string id)
    {
        try
        {
            if (await userManager.GetUserAsync(User) is not { } user) return Unauthorized();
            var decodedId = PublicToInternalId(id);
            var place = await dbContext.Places.FirstOrDefaultAsync(x => x.Id == decodedId && x.OwnerId == user.Id);
            if (place is null)
                return NotFound();

            return mapper.Map<PlaceDto>(place) with { Id = ToPublicId(place) };
        }
        catch (Exception ex)
        {
            logger.LogError(LogIds.Place.ControllerException, ex, "CreatePlace exception: {Message}", ex.Message);
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
            if (await userManager.GetUserAsync(User) is not { } user) return Unauthorized();

            var places = await dbContext.Places
                .IgnoreQueryFilters()
                .Where(x => x.IsSoftDeleted && x.OwnerId == user.Id)
                .ToListAsync();

            var dtos = places.Select(x => mapper.Map<PlaceDeletedDto>(x) with { Id = ToPublicId(x) });
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(LogIds.Place.ControllerException, ex, "CreatePlace exception: {Message}", ex.Message);
            return Problem();
        }
    }

    [HttpGet("Deleted/{id}")]
    [Authorize]
    [ProducesResponseType(typeof(PlaceDeletedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlaceDeletedDto>> GetDeletedPlace(string id)
    {
        try
        {
            if (await userManager.GetUserAsync(User) is not { } user) return Unauthorized();

            var decodedId = PublicToInternalId(id);
            var place = await dbContext.Places.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == decodedId && x.IsSoftDeleted && x.OwnerId == user.Id);
            if (place is null)
                return NotFound();

            var dto = mapper.Map<PlaceDeletedDto>(place) with { Id = ToPublicId(place) };
            return dto;
        }
        catch (Exception ex)
        {
            logger.LogError(LogIds.Place.ControllerException, ex, "CreatePlace exception: {Message}", ex.Message);
            return Problem();
        }
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(PlaceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> CreatePlace(CreatePlaceDto dto)
    {
        try
        {
            if (await userManager.GetUserAsync(User) is not { } user) return Unauthorized();

            var place = new Place(dto.Name, dto.Description ?? string.Empty,
                TimeProvider.System.GetUtcNow().UtcDateTime, user);

            dbContext.Places.Add(place);
            var count = await dbContext.SaveChangesAsync();

            if (count == 0) return BadRequest("Could not save place.");

            var publicId = ToPublicId(place);
            return CreatedAtAction(
                nameof(GetPlace),
                new { id = publicId },
                mapper.Map<PlaceDto>(place) with { Id = publicId }
            );
        }
        catch (Exception ex)
        {
            logger.LogError(LogIds.Place.ControllerException, ex, "CreatePlace exception: {Message}", ex.Message);
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
            var place = await dbContext.Places.FirstOrDefaultAsync(x => x.Id == decodedId);
            if (place is null)
                return NotFound();

            var result = place.Update(dto.Name, dto.Description);
            if (!result)
                return BadRequest("No update took place");

            var updated = await dbContext.SaveChangesAsync();

            return updated > 0 ? NoContent() : BadRequest("No update took place");
        }
        catch (Exception ex)
        {
            logger.LogError(LogIds.Place.ControllerException, ex, "CreatePlace exception: {Message}", ex.Message);
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
            var place = await dbContext.Places.FirstOrDefaultAsync(x => x.Id == decodedId);
            if (place is null)
                return NotFound();

            place.Delete();
            var updated = await dbContext.SaveChangesAsync();
            return updated > 0 ? NoContent() : BadRequest("No delete took place");
        }
        catch (Exception ex)
        {
            logger.LogError(LogIds.Place.ControllerException, ex, "CreatePlace exception: {Message}", ex.Message);
            return Problem();
        }
    }

    private int PublicToInternalId(string publicId)
    {
        var decoded = publicIdGenerator.Decode(publicId);
        return decoded.Count > 1 ? decoded[1] : 0;
    }

    private string ToPublicId(Place place)
    {
        return publicIdGenerator.Encode(PlaceConstants.PlacePublicIdAppendValue, place.Id);
    }
}