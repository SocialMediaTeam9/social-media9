using MediatR;

public class GetCommentsByContentQuery : IRequest<List<CommentDto>>
{
    public string ContentId { get; set; }

    public GetCommentsByContentQuery(string contentId)
    {
        ContentId = contentId;
    }
}
