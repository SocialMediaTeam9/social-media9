using Amazon.DynamoDBv2.DataModel; // For DynamoDB attributes
using System;

namespace social_media9.Api.Models
{
    // IMPORTANT: TableName must match the one configured in appsettings.json
    [DynamoDBTable("social_media9_Follows")]
    public class Follow
    {
        // FollowerId as Partition Key, FollowingId as Sort Key for efficient lookup
        [DynamoDBHashKey]
        public string FollowerId { get; set; } = string.Empty; // The user who is following (as string UUID)

        [DynamoDBRangeKey]
        public string FollowingId { get; set; } = string.Empty; // The user being followed (as string UUID)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}