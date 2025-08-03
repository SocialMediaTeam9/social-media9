using System;

namespace social_media9.Api.Dtos
{
    public class CommentDTO
    {
        public Guid CommentId { get; set; }
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
