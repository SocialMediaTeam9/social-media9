using MediatR;

namespace social_media9.Api.Commands
{
    public class DeleteCommentCommand : IRequest<bool>
    {
        public Guid CommentId { get; set; }
        public Guid PostId { get; set; }

        public DeleteCommentCommand(Guid commentId, Guid postId)
        {
            CommentId = commentId;
            PostId = postId;
        }
    }
}