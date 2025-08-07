using social_media9.Api.Models;
namespace social_media9.Api.Dtos
{
    public class CommentDto
    {
        public string CommentId { get; set; }
        public string PostId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        public CommentDto(Comment comment)
        {
            CommentId = comment.SK.Replace("COMMENT#", "");
            PostId = comment.PK.Replace("POST#", "");
            UserId = comment.UserId;
            Username = comment.Username;
            Content = comment.Content;
            CreatedAt = comment.CreatedAt;
        }
    }
}