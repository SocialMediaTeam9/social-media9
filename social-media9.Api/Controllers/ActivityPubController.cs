using Microsoft.AspNetCore.Mvc;
using MediatR;
using Newtonsoft.Json.Linq;
using social_media9.Api.Application.ActivityPub.Commands;
using social_media9.Api.Application.ActivityPub.Queries;

namespace social_media9.Api.Controllers;

[ApiController]
[Route(".well-known")]
public class WellKnownController : ControllerBase
{
    private readonly IMediator _mediator;

    public WellKnownController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("webfinger")]
    public async Task<IActionResult> WebFinger([FromQuery] string resource)
    {
        var result = await _mediator.Send(new ResolveWebFingerQuery(resource));
        return result is not null ? Ok(result) : NotFound();
    }
}

[ApiController]
[Route("users/{username}")]
public class ActivityPubController : ControllerBase
{
    private readonly IMediator _mediator;

    public ActivityPubController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetActor(string username)
    {
        var actor = await _mediator.Send(new GetActorProfileQuery(username));
        return actor is not null ? Ok(actor) : NotFound();
    }

    [HttpPost("inbox")]
    public async Task<IActionResult> Inbox(string username, [FromBody] JObject body)
    {
        await _mediator.Send(new HandleInboxActivityCommand(username, body));
        return Ok();
    }

    [HttpGet("outbox")]
    public async Task<IActionResult> Outbox(string username)
    {
        var result = await _mediator.Send(new GetOutboxActivitiesQuery(username));
        return Ok(result);
    }
}