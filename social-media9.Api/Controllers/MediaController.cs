using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using social_media9.Api.Models;

[ApiController]
[Route("/api/media")]
[Authorize] 
public class MediaController : ControllerBase
{
    private readonly S3Service _s3Service;

    public MediaController(S3Service s3Service)
    {
        _s3Service = s3Service;
    }

    [HttpPost("generate-upload-url")]
    public IActionResult GenerateUploadUrl([FromBody] GenerateUploadUrlRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (string.IsNullOrEmpty(request.FileName) || string.IsNullOrEmpty(request.ContentType))
        {
            return BadRequest("FileName and ContentType must be provided.");
        }

        var response = _s3Service.GetPresignedUploadUrl(userId, request.FileName, request.ContentType);
        
        return Ok(response);
    }
}