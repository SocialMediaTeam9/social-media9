using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using social_media9.Api.Services;
using social_media9.Api.DTOs;
using System.Security.Claims;
using social_media9.Api.Services.Interfaces;

namespace social_media9.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LikesController : ControllerBase
    {
        private readonly ILikeService _likeService;

        public LikesController(ILikeService likeService)
        {
            _likeService = likeService;
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
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error liking post: {ex.Message}");
            }
        }

        [HttpDelete]
        public async Task<ActionResult> UnlikePost([FromBody] UnlikePostRequest request)
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
                    return NotFound("Like not found");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error unliking post: {ex.Message}");
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
                return BadRequest($"Error getting post likes: {ex.Message}");
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
                return BadRequest($"Error getting posts liked status: {ex.Message}");
            }
        }
    }
}