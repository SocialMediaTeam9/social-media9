using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/recommendations")]
[Authorize]
public class RecommendationsController : ControllerBase
{
    private readonly RecommendationService _recommendationService;

    public RecommendationsController(RecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpGet("people-you-may-know")]
    public async Task<IActionResult> GetPeopleYouMayKnow()
    {
        var username = User.FindFirstValue(ClaimTypes.Name);
        if (username == null) return Unauthorized();

        var recommendations = await _recommendationService.GetPeopleYouMayKnowAsync(username);
        return Ok(recommendations);
    }
}