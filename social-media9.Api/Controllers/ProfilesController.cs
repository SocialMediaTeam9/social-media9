
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using social_media9.Api.Dtos;
using social_media9.Api.Models;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Services.DynamoDB;
using social_media9.Api.Services.Interfaces;

namespace social_media9.Api.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ProfilesController : ControllerBase
{
    private readonly IFederationService _federationService;
    private readonly DynamoDbService _dbService;
    private readonly IFollowRepository _followRepository;

    public ProfilesController(IFederationService federationService, DynamoDbService dbService, IFollowRepository followRepository)
    {
        _federationService = federationService;
        _dbService = dbService;
        _followRepository = followRepository;
    }


    [HttpGet("lookup")]
    [Authorize]
    public async Task<IActionResult> LookupUserProfile([FromQuery] string handle)
    {
        if (string.IsNullOrWhiteSpace(handle))
        {
            return BadRequest("Handle cannot be empty.");
        }

        User? user;
        handle = handle.TrimStart('@');

        if (handle.Contains('@') && !handle.Contains("@peerspace.online"))
        {
            user = await _federationService.DiscoverAndCacheUserAsync(handle);
        }
        else
        {

            var usernna = handle.Contains('@') ? handle.Split("@")[0] : handle;
            user = await _dbService.GetUserProfileByUsernameAsync(handle);
        }

        if (user == null)
        {
            return NotFound($"User '{handle}' could not be found.");
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool isFollowing = false;
        if (!string.IsNullOrEmpty(currentUserId) && currentUserId != user.UserId)
        {
            isFollowing = await _followRepository.IsFollowingAsync(currentUserId, user.UserId);
        }

        var userProfileDto = new UserProfileDto
        {
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            Bio = user.Bio,
            ProfilePictureUrl = user.ProfilePictureUrl,
            FollowersCount = user.FollowersCount,
            FollowingCount = user.FollowingCount,
            CreatedAt = user.CreatedAt,
            IsFollowing = isFollowing,
            ActorUrl = user.ActorUrl ?? ""
        };

        return Ok(userProfileDto);
    }
}