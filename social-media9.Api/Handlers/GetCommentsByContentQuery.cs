using MediatR;
using social_media9.Api.Models;


public class GetCommentsByContentQuery : IRequest<List<CommentDto>>
{
    public string ContentId { get; set; }

    public GetCommentsByContentQuery(string contentId)
    {
        ContentId = contentId;
    }
}
