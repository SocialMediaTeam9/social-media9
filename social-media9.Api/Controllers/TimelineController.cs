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
}