using Amazon.DynamoDBv2.DataModel;

namespace social_media9.Api.Models
{
    [DynamoDBTable("Posts")]
    public class Post
    {
        [DynamoDBHashKey]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Content { get; set; }
        public string? MediaUrl { get; set; }
        public string MediaType { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
public class Post
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Content { get; set; }
    public string? MediaUrl { get; set; } // Can be image or video URL
    public string MediaType { get; set; } // "image", "video", "none"
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}