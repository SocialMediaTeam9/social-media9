namespace social_media9.Api.Models
{
    public class UpdateCommentDto
    {
        public string CommentId { get; set; } 
        public string ContentId { get; set; }
        public string NewContent { get; set; }
    }
}