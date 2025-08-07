using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using social_media9.Api.Dtos;
using social_media9.Api.Services.Interfaces;
using social_media9.Api.Models;
using social_media9.Api.Services.Implementations;
using social_media9.Api.Repositories.Interfaces;
using System.Security.Claims;
using MediatR;
using social_media9.Api.Queries.Posts;

namespace social_media9.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly PostService _postService;
        private readonly IUserRepository _userRepository;

        private readonly IMediator _mediator;

        public PostsController(PostService postService, IUserRepository userRepository, IMediator mediator)
        {
            _postService = postService;
            _userRepository = userRepository;
            _mediator = mediator;
        }

        private string GetCurrentUsername() => User.FindFirstValue(ClaimTypes.Name)!;

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var user = await _userRepository.GetUserByIdAsync(userIdClaim);

            if (user == null)
            {
                return Unauthorized("User not found.");
            }


            var post = await _postService.CreateAndFederatePostAsync(user.Username, request.Content, request.AttachmentUrls);

            if (post == null)
            {
                return StatusCode(500, "Failed to create post.");
            }

            var response = new PostResponse(
                PostId: post.SK.Replace("POST#", ""),
                AuthorUsername: post.AuthorUsername,
                Content: post.Content,
                CreatedAt: post.CreatedAt,
                CommentCount: post.CommentCount
            );

            return CreatedAtAction(nameof(GetPost), new { postId = response.PostId }, response);
        }

        [HttpGet("by/{username}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPostsByUsername(
        string username,
        [FromQuery] int limit = 15,
        [FromQuery] string? cursor = null)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest("Username cannot be empty.");
            }

            try
            {
                var query = new GetPostsByUserQuery(username, limit, cursor);
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving posts." });
            }
        }

        // GET /api/posts/{postId}
        [HttpGet("{postId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPost(String postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null)
            {
                return NotFound();
            }

            var response = new PostResponse(
                PostId: post.SK.Replace("POST#", ""),
                AuthorUsername: post.AuthorUsername,
                Content: post.Content,
                CreatedAt: post.CreatedAt,
                CommentCount: post.CommentCount
            );

            return Ok(response);
        }

        // GET /api/users/{userId}/posts
        [HttpGet("{username}/posts")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserPosts(Guid userId)
        {
            var posts = await _postService.GetUserPostsAsync(userId.ToString());
            return Ok(posts);
        }

        // POST /api/posts/{postId}/like
        [HttpPost("{postId}/like")]
        [Authorize]
        public async Task<IActionResult> LikePost(string postId)
        {
            var likerUsername = GetCurrentUsername();
            var success = await _postService.LikePostAsync(postId, likerUsername);
            if (!success)
            {
                return BadRequest(new { message = "Post not found or you have already liked this post." });
            }
            return Ok(new { message = "Post liked successfully." });
        }

        // GET /api/posts/{postId}/likes
        [HttpGet("{postId}/likes")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPostLikes(Guid postId)
        {
            // TODO: Implement get likes logic
            return Ok();
        }

        // POST /api/posts/{postId}/comments
        [HttpPost("{postId}/comments")]
        // [Authorize]
        public async Task<IActionResult> AddComment(Guid postId, [FromBody] AddCommentRequest request)
        {
            // TODO: Implement add comment logic in service and call here
            return StatusCode(201);
        }

        // GET /api/posts/{postId}/comments
        [HttpGet("{postId}/comments")]
        [AllowAnonymous]
        public async Task<IActionResult> GetComments(Guid postId)
        {
            // TODO: Implement get comments logic in service and call here
            return Ok();
        }
    }
}