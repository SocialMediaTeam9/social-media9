using MediatR;
using social_media9.Api.Models;
using social_media9.Api.Dtos;


public class GetCommentsByContentQuery : IRequest<List<CommentDto>>
{
    public Guid PostId { get; set; }

    public GetCommentsByContentQuery(Guid PostId)
    {
        this.PostId = PostId;
    }
}
