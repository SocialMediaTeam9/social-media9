using MediatR;
using social_media9.Api.Models;
using social_media9.Api.Dtos;


public class GetCommentsByContentQuery : IRequest<List<CommentResponse>>
{
    public string PostId { get; set; }

    public GetCommentsByContentQuery(string PostId)
    {
        this.PostId = PostId;
    }
}
