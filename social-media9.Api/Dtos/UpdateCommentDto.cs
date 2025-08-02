namespace social_media9.Api.Dtos
{
    public class UpdateCommentDto
    {
        public Guid CommentId { get; set; } 
        public Guid PostId { get; set; }
        public string NewContent { get; set; }
    }
}