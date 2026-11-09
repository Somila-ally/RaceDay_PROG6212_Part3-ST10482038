using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;
using RaceDay.Api.Security;
using RaceDay.Api.Services;
using RaceDay.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api/images"), RequireRole]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[SwaggerTag("Uploads event banners and profile images to Azure Blob Storage. Only JPEG, PNG and WebP images up to 5 MB are accepted.")]
public sealed class ImagesController(RaceDayDbContext db) : ControllerBase
{
    /// <summary>Uploads and saves an event banner image for an event owned by the logged-in Organiser.</summary>
    /// <response code="200">Image uploaded to Azure Blob Storage and URL saved on the event.</response>
    /// <response code="400">File missing, unsupported type, or larger than 5 MB.</response>
    /// <response code="403">Organiser does not own the event.</response>
    /// <response code="404">Event not found.</response>
    /// <response code="503">Azure Blob Storage is not configured.</response>
    [HttpPost("events/{eventId:int}/banner"), RequireRole(RoleNames.Organiser), Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status403Forbidden), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UploadBanner(int eventId, IFormFile? file, [FromServices] BlobStorageService storage, CancellationToken cancellationToken)
    {
        var ev = await db.Events.FirstOrDefaultAsync(e => e.EventId == eventId, cancellationToken);
        if (ev is null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserId != HttpContext.Session.GetInt32(SessionKeys.UserId)) return Forbid();
        try { ev.BannerImageUrl = await storage.UploadAsync(file, $"events/{eventId}", cancellationToken); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message }); }
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new UploadResponse { FileName = Path.GetFileName(new Uri(ev.BannerImageUrl).AbsolutePath), Url = ev.BannerImageUrl });
    }

    /// <summary>Uploads and saves a profile image for the logged-in Organiser or Participant.</summary>
    /// <response code="200">Image uploaded to Azure Blob Storage and URL saved on the profile.</response>
    /// <response code="400">File missing, unsupported type, or larger than 5 MB.</response>
    /// <response code="503">Azure Blob Storage is not configured.</response>
    [HttpPost("profile"), Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UploadProfileImage(IFormFile? file, [FromServices] BlobStorageService storage, CancellationToken cancellationToken)
    {
        var id = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);
        if (user is null) return Unauthorized();
        try { user.ProfileImageUrl = await storage.UploadAsync(file, $"profiles/{id}", cancellationToken); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message }); }
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new UploadResponse { FileName = Path.GetFileName(new Uri(user.ProfileImageUrl).AbsolutePath), Url = user.ProfileImageUrl });
    }
}
