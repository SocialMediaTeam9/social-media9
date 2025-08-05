using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using social_media9.Api.Models;
using social_media9.Api.Services.DynamoDB;

namespace social_media9.Api.Controllers;

[ApiController]
[Route("/internal/v1")]
[Authorize(Policy = "InternalApi")]
public class FederationController : ControllerBase
{
    private readonly DynamoDbService _dbService;
    private readonly ILogger<FederationController> _logger;

    public FederationController(DynamoDbService dbService, ILogger<FederationController> logger)
    {
        _dbService = dbService;
        _logger = logger;
    }



    [HttpPost("user")]
    public async Task<IActionResult> GetUserInfo([FromBody] GtsUserInfoRequest request)
    {
        _logger.LogInformation("GetUserInfo internal api");
        var userEntity = await _dbService.GetUserProfileByUsernameAsync(request.Username);

        if (userEntity == null)
        {
            _logger.LogWarning($"User '{request.Username}' not found.");
            return NotFound(new { error = $"User '{request.Username}' not found." });
        }

        var response = new GtsUserInfoResponse(
            Username: userEntity.Username,
            DisplayName: userEntity.Username,
            PublicKey: userEntity.PublicKeyPem,
            PrivateKey: userEntity.PrivateKeyPem
        );

        _logger.LogInformation(response.ToString());

        return Ok(response);
    }

    [HttpPost("followers")]
    public async Task<IActionResult> GetFollowers([FromBody] GtsCollectionRequest request)
    {
        var followerEntities = await _dbService.GetFollowersAsync(request.Username);
        var followerUrls = followerEntities
            .Select(entity => entity.FollowerInfo.ActorUrl)
            .ToList();

        var response = new GtsCollectionResponse(followerUrls);
        return Ok(response);
    }

    [HttpPost("following")]
    public async Task<IActionResult> GetFollowing([FromBody] GtsCollectionRequest request)
    {
        var followingEntities = await _dbService.GetFollowingAsync(request.Username);
        var followingUrls = followingEntities
            .Select(entity => entity.FollowingInfo.ActorUrl)
            .ToList();

        var response = new GtsCollectionResponse(followingUrls);
        return Ok(response);
    }
}