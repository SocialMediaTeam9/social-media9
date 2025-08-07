using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
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
        // var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = "testuser"; // For testing purposes, replace with actual user retrieval logic
        // if (string.IsNullOrEmpty(username))
        // {
        //     return Unauthorized();
        // }

        var timeline = await _timelineService.GetUserTimelineAsync(username, limit, cursor);

        return Ok(timeline);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<PaginatedTimelineResponse>> GetTimeline(string username, [FromQuery] string? cursor = null)
    {
        var timeline = await _timelineService.GetTimelineAsync(username, cursor);
        return Ok(timeline);
    }

    [HttpPost("/users/{username}/inbox")]
    public async Task<IActionResult> ReceiveActivity(string username)
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        if (Request.ContentType != "application/activity+json")
        {
            return BadRequest("Invalid content type");
        }

        try
        {
            var activity = JsonSerializer.Deserialize<JsonElement>(body);

            var type = activity.GetProperty("type").GetString();
            if (type == "Create" && activity.TryGetProperty("object", out var obj))
            {
                var content = obj.GetProperty("content").GetString();
                var author = activity.GetProperty("actor").GetString();

                await _timelineService.ProcessIncomingPostAsync(username, content ?? "", author ?? "unknown");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest($"Invalid JSON: {ex.Message}");
        }
    }
}