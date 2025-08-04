using social_media9.Api.Models;
namespace social_media9.Api.Dtos
{
    public class CommentDto
    {
        public string CommentId { get; set; }
        public string PostId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }

        public CommentDto() { }

        // public CommentDto(Comment comment)
        // {
        //     CommentId = comment.CommentId;
        //     PostId = comment.PostId;
        //     UserId = comment.UserId;
        //     Username = comment.Username;
        //     Text = comment.Content;
        //     CreatedAt = comment.CreatedAt;
        // }
    }
}