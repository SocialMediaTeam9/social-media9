using Amazon.DynamoDBv2.DataModel;
using social_media9.Api.Models.DynamoDb;
using System;
using System.Text.Json;

namespace social_media9.Api.Models
{
    [DynamoDBTable("nexusphere-mvp-main-table")]
    public class Post : BaseEntity
    {
        // [DynamoDBHashKey]
        // public Guid Id { get; set; } = Guid.NewGuid();
        // public string Content { get; set; }
        public string? MediaUrl { get; set; }
        public string MediaType { get; set; }
        public Guid UserId { get; set; }

        [DynamoDBProperty("AuthorUsername")]
        public string AuthorUsername { get; set; } = string.Empty;

        [DynamoDBProperty("Content")]
        public string Content { get; set; } = string.Empty;

        [DynamoDBProperty("ActivityJson")]
        public string ActivityJson { get; set; } = string.Empty;

        [DynamoDBProperty("CommentCount")]
        public int CommentCount { get; set; } = 0;

        [DynamoDBProperty("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DynamoDBProperty("Attachments")]
        public List<string> Attachments { get; set; } = new();

        public static Post FromActivityPub(JsonElement postObject, string authorUsername)
        {
            var postId = new Uri(postObject.GetProperty("id").GetString()!).Segments.Last();
            
            return new Post
            {
                PK = $"POST#{postId}",
                SK = $"POST#{postId}",
                GSI1PK = $"USER#{authorUsername}",
                GSI1SK = $"POST#{postId}",
                AuthorUsername = authorUsername,
                Content = postObject.GetProperty("content").GetString() ?? "",
                ActivityJson = postObject.ToString(),
                CreatedAt = postObject.TryGetProperty("published", out var published) ? published.GetDateTime() : DateTime.UtcNow,
                // You can also parse attachments here
            };
    }
    }
}