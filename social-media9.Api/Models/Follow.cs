using Amazon.DynamoDBv2.DataModel;
using social_media9.Api.Models.DynamoDb;
using System;

namespace social_media9.Api.Models
{

    [DynamoDBTable("nexusphere-mvp-main-table")]
    public class Follow : BaseEntity
    {
        public Follow() { Type = "Follow"; }

        // [DynamoDBHashKey]
        // public string FollowerId { get; set; } = string.Empty;

        // [DynamoDBRangeKey]
        // public string FollowingId { get; set; } = string.Empty;

        // public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



        [DynamoDBProperty("FollowerInfo")]
        public UserSummary FollowerInfo { get; set; } = new();

        [DynamoDBProperty("FollowingInfo")]
        public UserSummary FollowingInfo { get; set; } = new();

        [DynamoDBProperty("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public static Follow Create(UserSummary follower, UserSummary following)
        {

            string localDomain = Environment.GetEnvironmentVariable("DomainName") ?? "peerspace.online";

            string followerDomain = GetDomainFromActorUrl(follower.ActorUrl, localDomain);
            string followingDomain = GetDomainFromActorUrl(following.ActorUrl, localDomain);

            string followerId = $"{follower.Username}@{followerDomain}";
            string followingId = $"{following.Username}@{followingDomain}";

            var followerPk = $"USER#{followerId}";
            var followingPk = $"USER#{followingId}";

            return new Follow
            {
                PK = followerPk,
                SK = $"FOLLOWS#{following.ActorUrl}",
                GSI1PK = followingPk,
                GSI1SK = $"FOLLOWED_BY#{follower.ActorUrl}",
                FollowerInfo = follower,
                FollowingInfo = following
            };
        }

        private static string GetDomainFromActorUrl(string actorUrl, string localDomain)
        {
            if (string.IsNullOrEmpty(actorUrl))
                return localDomain;

            try
            {
                var uri = new Uri(actorUrl);
                return uri.Host;
            }
            catch
            {
                return localDomain;
            }
        }
    }
}