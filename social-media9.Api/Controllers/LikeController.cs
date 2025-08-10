using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using social_media9.Api.Services.Interfaces;
using social_media9.Api.DTOs;
using System.Security.Claims;

namespace social_media9.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LikesController : ControllerBase
    {
        private readonly ILikeService _likeService;
        private readonly ILogger<LikesController> _logger;

        public LikesController(ILikeService likeService, ILogger<LikesController> logger)
        {
            _likeService = likeService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<LikeResponse>> LikePost([FromBody] LikePostRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _likeService.LikePostAsync(request.PostId, userId, username);
                if (string.IsNullOrEmpty(result.PostId))
                {
                    return BadRequest("Unable to like post.");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking post {PostId}", request.PostId);
                return StatusCode(500, "An error occurred while liking the post.");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> UnlikePost([FromBody] UnlikePostRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                var success = await _likeService.UnlikePostAsync(request.PostId, userId);
                if (!success)
                {
                    return NotFound("Like not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unliking post {PostId}", request.PostId);
                return StatusCode(500, "An error occurred while unliking the post.");
            }
        }

        [HttpGet("{postId}")]
        public async Task<ActionResult<PostLikesResponse>> GetPostLikes(string postId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                var result = await _likeService.GetPostLikesAsync(postId, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving likes for post {PostId}", postId);
                return StatusCode(500, "An error occurred while retrieving post likes.");
            }
        }

        [HttpPost("batch-status")]
        public async Task<ActionResult<Dictionary<string, bool>>> GetPostsLikedStatus([FromBody] List<string> postIds)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _likeService.GetPostsLikedStatusAsync(postIds, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving liked status for multiple posts");
                return StatusCode(500, "An error occurred while retrieving liked status.");
            }
        }
    }
}