using MediatR;
using social_media9.Api.Dtos;

namespace social_media9.Api.Commands
{
    public class AddCommentCommand : IRequest<CommentDto>
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Text { get; set; }
    }
}