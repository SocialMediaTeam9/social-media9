using System;
using Amazon.DynamoDBv2.DataModel;

namespace social_media9.Api.Models
{
    [DynamoDBTable("Likes")]
    public class Like
    {
        [DynamoDBHashKey]
        public Guid PostId { get; set; }
        [DynamoDBRangeKey]
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
