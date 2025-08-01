using MediatR;

public class UpdateCommentCommand : IRequest<bool>
{
    public string CommentId { get; set; }
    public string ContentId { get; set; }
    public string NewContent { get; set; }
}
