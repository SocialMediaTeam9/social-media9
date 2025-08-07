using MediatR;
using social_media9.Api.Models;
using social_media9.Api.Dtos;


public class GetCommentByCommentIdQuery : IRequest<CommentDto>
{
    public Guid CommentId { get; set; }

    public GetCommentsByContentQuery(Guid CommentId)
    {
        this.CommentId = CommentId;
    }
}