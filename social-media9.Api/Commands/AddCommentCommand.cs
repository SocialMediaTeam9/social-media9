using MediatR;
using social_media9.Api.Models;

namespace social_media9.Api.Commands
{
    public class AddCommentCommand : IRequest<CommentDto>
    {
        public string ContentId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Text { get; set; }
    }
}