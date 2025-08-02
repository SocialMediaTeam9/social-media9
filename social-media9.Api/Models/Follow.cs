using Amazon.DynamoDBv2.DataModel;
using System;

namespace social_media9.Api.Models
{

    [DynamoDBTable("social_media9_Follows")]
    public class Follow
    {
        
        [DynamoDBHashKey]
        public string FollowerId { get; set; } = string.Empty; 

        [DynamoDBRangeKey]
        public string FollowingId { get; set; } = string.Empty; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}