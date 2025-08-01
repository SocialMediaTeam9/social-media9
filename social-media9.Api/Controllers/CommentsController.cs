using Microsoft.AspNetCore.Mvc;
using MediatR;

[ApiController]
[Route("api/comments")]
public class CommentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CommentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> AddComment([FromBody] AddCommentCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetComments), new { contentId = result.ContentId }, result);
    }

    [HttpGet("{contentId}")]
    public async Task<IActionResult> GetComments(string contentId)
    {
        var result = await _mediator.Send(new GetCommentsByContentQuery(contentId));
        return Ok(result);
    }

    [HttpDelete("{contentId}/{commentId}")]
    public async Task<IActionResult> DeleteComment(string contentId, string commentId)
    {
        await _mediator.Send(new DeleteCommentCommand(commentId, contentId));
        return NoContent();
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateComment([FromBody] UpdateCommentDto dto)
    {
        var command = new UpdateCommentCommand
        {
            CommentId = dto.CommentId,
            ContentId = dto.ContentId,
            NewContent = dto.NewContent
        };

        var result = await _mediator.Send(command);
        return result ? Ok("Updated") : BadRequest("Update failed");
    }
}
