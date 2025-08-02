namespace social_media9.Api.Models
{
    public class Comment
    {
        public string CommentId { get; set; }
        public string ContentId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}