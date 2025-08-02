using MediatR;
using Microsoft.AspNetCore.Mvc;
using social_media9.Api.Handlers.Search;

namespace social_media9.Api.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
  private readonly IMediator _mediator;

  public SearchController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpGet("content")]
  public async Task<IActionResult> SearchContent([FromQuery] string q, [FromQuery] int limit = 20)
  {
    if (string.IsNullOrWhiteSpace(q))
    {
      return BadRequest("Search query 'q' cannot be empty.");
    }
    var query = new SearchContentQuery(q, limit);
    var results = await _mediator.Send(query);
    return Ok(results);
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