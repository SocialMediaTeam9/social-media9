public class CommentDto
{
    public string CommentId { get; set; }
    public string ContentId { get; set; }
    public string UserId { get; set; }
    public string Username { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }

    public CommentDto() {}

    public CommentDto(Comment comment)
    {
        CommentId = comment.CommentId;
        ContentId = comment.ContentId;
        UserId = comment.UserId;
        Username = comment.Username;
        Text = comment.Text;
        CreatedAt = comment.CreatedAt;
    }
}
