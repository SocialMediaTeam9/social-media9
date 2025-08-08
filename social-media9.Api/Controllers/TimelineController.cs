using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TimelineController : ControllerBase
{
    private readonly ITimelineService _timelineService;

    public TimelineController(ITimelineService timelineService)
    {
        _timelineService = timelineService;
    }


    [HttpGet("home")]
    public async Task<IActionResult> GetHomeTimeline([FromQuery] int limit = 20, [FromQuery] string? cursor = null)
    {
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        var timeline = await _timelineService.GetUserTimelineAsync(username, limit, cursor);

        return Ok(timeline);
    }

    [HttpGet("public")]
    [AllowAnonymous] // Allow anyone to see the public feed
    public async Task<IActionResult> GetPublicTimeline([FromQuery] int limit = 20, [FromQuery] string? cursor = null)
    {
        var timeline = await _timelineService.GetPublicTimelineAsync(limit, cursor);
        return Ok(timeline);
    }
}