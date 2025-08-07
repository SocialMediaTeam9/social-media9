using Amazon.DynamoDBv2.DataModel;
using social_media9.Api.Models.DynamoDb;
using System;

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
        // public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


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
    }
}