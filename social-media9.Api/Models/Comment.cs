using System;
using Amazon.DynamoDBv2.DataModel;
using social_media9.Api.Models.DynamoDb;

namespace social_media9.Api.Models
{
    [DynamoDBTable("nexusphere-mvp-main-table")]
    public class Comment : BaseEntity
    {
        // [DynamoDBHashKey] // Partition key
        // public string PostId { get; set; }

        // [DynamoDBRangeKey] // Sort key
        // public string CommentId { get; set; } = Guid.NewGuid().ToString();

        // public string UserId { get; set; }
        // public string Username { get; set; } = string.Empty;
        // public string Text { get; set; } = string.Empty;
        // public DateTime CreatedAt { get; set; }

        [DynamoDBProperty("AuthorUsername ")]
        public string Username { get; set; } = string.Empty;

        [DynamoDBProperty("Content")]
        public string Content { get; set; } = string.Empty;

        [DynamoDBProperty("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
    }

}

