using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using social_media9.Api.Queries.SearchContent;
using social_media9.Api.Queries.SearchUsers;
using social_media9.Api.Queries.SearchHashtags;

namespace social_media9.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // It's good practice to protect search endpoints
    public class SearchController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SearchController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("users")]
        public async Task<IActionResult> SearchUsers([FromQuery] string q, [FromQuery] int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest("Search query 'q' cannot be empty.");
            }
            var query = new SearchUsersQuery(q, limit);
            var results = await _mediator.Send(query);
            return Ok(results);
        }
        
        [HttpGet("posts")]
        public async Task<IActionResult> SearchPosts([FromQuery] string q, [FromQuery] int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest("Search query 'q' cannot be empty.");
            }
            var query = new SearchContentQuery(q, limit); // Reusing the content query
            var results = await _mediator.Send(query);
            return Ok(results);
        }

        [HttpGet("hashtags")]
        public async Task<IActionResult> SearchHashtags([FromQuery] string q, [FromQuery] int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest("Search query 'q' cannot be empty.");
            }
            var query = new SearchHashtagsQuery(q, limit);
            var results = await _mediator.Send(query);
            return Ok(results);
        }
    }
}