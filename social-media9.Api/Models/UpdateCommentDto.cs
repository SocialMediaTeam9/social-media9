public class UpdateCommentDto
{
    public string CommentId { get; set; } // Composite key: PK or SK
    public string ContentId { get; set; }
    public string NewContent { get; set; }
}
