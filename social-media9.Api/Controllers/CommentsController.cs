using Microsoft.AspNetCore.Mvc;
using MediatR;
using social_media9.Api.Models;
using social_media9.Api.Commands;
using social_media9.Api.Dtos;
using System.Security.Claims;
using social_media9.Api.Repositories.Interfaces;

namespace social_media9.Api.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IUserRepository _userRepository;
        private readonly ICommentRepository _commentRepository;

        public CommentsController(IMediator mediator, IUserRepository userRepository, ICommentRepository commentRepository)
        {
            _mediator = mediator;
            _userRepository = userRepository;
            _commentRepository = commentRepository;
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
            if (!IsUserAuthorized(postId,commentId))
            {
                return Unauthorized("You are not authorized to delete this comment.");
            }
            await _mediator.Send(new DeleteCommentCommand(commentId, postId));
            return NoContent();
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateComment([FromBody] UpdateCommentDto dto)
        {
            if (!IsUserAuthorized(dto.PostId,dto.CommentId))
            {
                return Unauthorized("You are not authorized to update this comment.");
            }
            
            var command = new UpdateCommentCommand
            {
                CommentId = dto.CommentId,
                PostId = dto.PostId,
                NewContent = dto.NewContent
            };

            var result = await _mediator.Send(command);
            return result ? Ok("Updated") : BadRequest("Update failed");
        }
        public bool IsUserAuthorized(Guid postId,Guid commentId)
        {
                        
            string? googleId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string userId = _userRepository.GetUserByGoogleIdAsync(googleId).Result?.UserId ?? string.Empty;
            var comment = _commentRepository.GetCommentByIdAsync(postId,commentId).Result;
            
            if (comment == null)
            {
                return false;
            }
            else
            {
                return comment.UserId == userId;
            }

        }
    }

}