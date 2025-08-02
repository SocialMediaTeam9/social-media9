using MediatR;

namespace social_media9.Api.Commands
{
    public class UpdateCommentCommand : IRequest<bool>
    {
        public Guid CommentId { get; set; }
        public Guid PostId { get; set; }
        public string NewContent { get; set; }
    }
}