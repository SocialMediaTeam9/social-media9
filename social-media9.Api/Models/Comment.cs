using System;
using Amazon.DynamoDBv2.DataModel;
using social_media9.Api.Models.DynamoDb;

namespace social_media9.Api.Models
{
    [DynamoDBTable("nexusphere-mvp-main-table")]
    public class Comment : BaseEntity
    {
        public string UserId { get; set; }

        [DynamoDBProperty("AuthorUsername ")]
        public string Username { get; set; } = string.Empty;

        [DynamoDBProperty("Content")]
        public string Text { get; set; } = string.Empty;

        [DynamoDBProperty("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
    }

}

