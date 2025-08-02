using MediatR;
using social_media9.Api.Models;


public class GetCommentsByContentQuery : IRequest<List<CommentDto>>
{
    public Guid PostId { get; set; }

    public GetCommentsByContentQuery(Guid PostId)
    {
        this.PostId = PostId;
    }
}
