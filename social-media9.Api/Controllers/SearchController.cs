using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        /// Performs a unified search for local users, remote users (via federation), and local posts.
        /// </summary>
        /// <param name="q">The search query.</param>
        /// <param name="limit">The maximum number of results to return.</param>
        [HttpGet] // This is now the main endpoint: /api/search
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 20)
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