using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
    [Authorize]
    public async Task<IActionResult> CreatePost([FromForm] CreatePostRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        var userId = Guid.Parse(userIdClaim.Value);
        var postId = await _postService.CreatePostAsync(request, userId);
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
        // TODO: Implement get post logic
        return Ok();
    }

    // GET /api/users/{userId}/posts
    [HttpGet("/api/users/{userId}/posts")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserPosts(Guid userId)
    {
        // TODO: Implement get all posts by user
        return Ok();
    }

    // POST /api/posts/{postId}/like
    [HttpPost("{postId}/like")]
    [Authorize]
    public async Task<IActionResult> LikePost(Guid postId)
    {
        // TODO: Implement like logic
        return Ok();
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
    [Authorize]
    public async Task<IActionResult> AddComment(Guid postId, [FromBody] AddCommentRequest request)
    {
        // TODO: Implement add comment logic
        return StatusCode(201);
    }

    // GET /api/posts/{postId}/comments
    [HttpGet("{postId}/comments")]
    [AllowAnonymous]
    public async Task<IActionResult> GetComments(Guid postId)
    {
        // TODO: Implement get comments logic
        return Ok();
    }
}
}