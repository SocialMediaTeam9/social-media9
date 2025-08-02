using Amazon.DynamoDBv2.DataModel;
using System;

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