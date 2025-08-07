using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using social_media9.Api.Dtos;
using social_media9.Api.Services.Interfaces;

namespace social_media9.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpPost]
        // [Authorize]
        [AllowAnonymous]
        public async Task<IActionResult> CreatePost(
            [FromForm] CreatePostRequest request,
            [FromForm] IFormFile? mediaFile)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { message = "Validation failed", errors });
            }

            // TODO: Replace hardcoded userId with actual authenticated userId
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var postId = await _postService.CreatePostAsync(request, userId, mediaFile);

            return Ok(new { PostId = postId });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPosts()
        {
            var posts = await _postService.GetPostsAsync();
            return Ok(posts);
        }

        // GET /api/posts/{postId}
        [HttpGet("{postId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPost(Guid postId)
        {
            var post = await _postService.GetPostAsync(postId);

            if (post == null)
                return NotFound(new { message = "Post not found." });

            return Ok(post);
        }

        // GET /api/users/{userId}/posts
        [HttpGet("/api/users/{userId}/posts")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserPosts(Guid userId)
        {
            var posts = await _postService.GetUserPostsAsync(userId);
            return Ok(posts);
        }

        // POST /api/posts/{postId}/like
        [HttpPost("{postId}/like")]
        [AllowAnonymous] // Change to [Authorize] when authentication is added
        public async Task<IActionResult> LikePost(Guid postId)
        {
            // TODO: Replace hardcoded userId with actual authenticated userId
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            var result = await _postService.LikePostAsync(postId, userId);
            if (result)
                return Ok(new { message = "Post liked." });
            else
                return BadRequest(new { message = "Could not like post." });
        }

        // GET /api/posts/{postId}/likes
        [HttpGet("{postId}/likes")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPostLikes(Guid postId)
        {
            // TODO: Implement get likes logic in service and call here
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