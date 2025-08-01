using MediatR;

public class DeleteCommentCommand : IRequest<bool>
{
    public string CommentId { get; set; }
    public string ContentId { get; set; }

    public DeleteCommentCommand(string commentId, string contentId)
    {
        CommentId = commentId;
        ContentId = contentId;
    }
}
