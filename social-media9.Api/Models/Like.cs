using System;
using Amazon.DynamoDBv2.DataModel;

namespace social_media9.Api.Models
{
    [DynamoDBTable("nexusphere-mvp-main-table")]
    public class Like
    {
        [DynamoDBHashKey]
        public string PostId { get; set; } = string.Empty;
        
        [DynamoDBRangeKey]
        public string UserId { get; set; } = string.Empty;
        
        [DynamoDBProperty]
        public string Username { get; set; } = string.Empty;
        
        [DynamoDBProperty]
        public DateTime CreatedAt { get; set; }
        
        [DynamoDBProperty]
        public string LikeId { get; set; } = string.Empty;
    }
}
