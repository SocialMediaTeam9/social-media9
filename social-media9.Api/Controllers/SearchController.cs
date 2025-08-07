using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using social_media9.Api.Queries.SearchUsers;
using social_media9.Api.Queries.SearchAll;

namespace social_media9.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SearchController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Searches specifically for users (local and federated).
        /// Used by the "Users" tab on the search page.
        /// </summary>
        /// <param name="q">The search query (e.g., "alice" or "bob@anotherserver.com").</param>
        /// <param name="limit">The maximum number of results to return.</param>
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
        
        /// <summary>
        /// Performs a general search for users, posts, and hashtags.
        /// Used by the main "Search" tab on the search page.
        /// </summary>
        /// <param name="q">The search query (e.g., "alice", "#fediverse", "hello world").</param>
        /// <param name="limit">The maximum number of results to return.</param>
        [HttpGet("all")]
        public async Task<IActionResult> SearchAll([FromQuery] string q, [FromQuery] int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest("Search query 'q' cannot be empty.");
            }
            var query = new SearchAllQuery(q, limit);
            var results = await _mediator.Send(query);
            return Ok(results);
        }
    }
}