
using Microsoft.AspNetCore.Mvc;
using social_media9.Api.Repositories.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class FollowController : ControllerBase
{
    private readonly IFollowRepository _followRepository;

    public FollowController(IFollowRepository followRepository)
    {
        _followRepository = followRepository;
    }

    [HttpPost("{followerId}/follow/{followingId}")]
    public async Task<IActionResult> Follow(string followerId, string followingId)
    {
        await _followRepository.FollowAsync(followerId, followingId);
        return Ok(new { message = "Followed successfully" });
    }


}
