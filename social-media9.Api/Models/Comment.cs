using System;
using Amazon.DynamoDBv2.DataModel;

namespace social_media9.Api.Models
{
    [DynamoDBTable("social_media9_Comments")]
    public class Comment
    {
        [DynamoDBHashKey] // Partition key
        public string PostId { get; set; } 

        [DynamoDBRangeKey] // Sort key
        public string  CommentId { get; set; } = Guid.NewGuid().ToString();

        public string UserId { get; set; } 
        public string Username { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

}

