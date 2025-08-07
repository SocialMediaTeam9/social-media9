using Microsoft.AspNetCore.Mvc;
using MediatR;
using social_media9.Api.Models;
using social_media9.Api.Commands;
using social_media9.Api.Dtos;



namespace social_media9.Api.Controllers
{
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

            return CreatedAtAction(nameof(GetComments), new { result.PostId }, result);
        }

        [HttpGet("{postId}")]
        public async Task<IActionResult> GetComments([FromRoute] string postId)
        {
            var result = await _mediator.Send(new GetCommentsByContentQuery(postId));
            return Ok(result);
        }

        [HttpDelete("{postId}/{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid postId, Guid commentId)
        {
            await _mediator.Send(new DeleteCommentCommand(commentId, postId));
            return NoContent();
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateComment([FromBody] UpdateCommentDto dto)
        {
            var command = new UpdateCommentCommand
            {
                CommentId = dto.CommentId,
                PostId = dto.PostId,
                NewContent = dto.NewContent
            };

            var result = await _mediator.Send(command);
            return result ? Ok("Updated") : BadRequest("Update failed");
        }
    }
}