using MediatR;
using social_media9.Api.Dtos;

namespace social_media9.Api.Commands
{
    public class AddCommentCommand : IRequest<CommentResponse>
    {
        public string PostId { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
    }
}

public record CreateCommentRequest(string Content);

public record CommentResponse(
    string CommentId,
    string PostId,
    string AuthorUsername,
    string Content,
    DateTime CreatedAt
);