using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Domain.Exceptions;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecruiterController : ControllerBase
{
    private readonly IRecruiterService _recruiterService;
    private readonly ILogger<RecruiterController> _logger;

    public RecruiterController(IRecruiterService recruiterService, ILogger<RecruiterController> logger)
    {
        _recruiterService = recruiterService;
        _logger = logger;
    }

    /// <summary>
    /// Update recruiter profile picture
    /// </summary>
    [HttpPut("profile-picture")]
    [Authorize(Roles = "Recruiter,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<string>> UpdateProfilePicture(IFormFile file)
    {
        var recruiterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(recruiterId))
            return Unauthorized();

        // Validate file
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Image file is required and cannot be empty" });

        // Extension and size validation is handled by FileStorageService

        _logger.LogInformation("Profile picture upload for recruiter {UserId}: {FileName}", recruiterId, file.FileName);

        try
        {
            using (var stream = file.OpenReadStream())
            {
                var picturePath = await _recruiterService.UpdateProfilePictureAsync(recruiterId, file.FileName, stream);
                return Ok(new { picturePath });
            }
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload profile picture for recruiter {UserId}", recruiterId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while uploading the picture" });
        }
    }
}
