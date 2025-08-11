using Gremlin.Net.Driver;
using Gremlin.Net.Structure;
using Gremlin.Net.Structure.IO.GraphSON;
using Gremlin.Net.Process.Traversal;
using social_media9.Api.Models;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Dtos;
using Neo4j.Driver;


public class NeptuneFollowRepository : IFollowRepository
{
    private readonly IDriver _client;

    public NeptuneFollowRepository(IDriver driver)
    {
        _client = driver ?? throw new ArgumentNullException(nameof(driver));
    }

    public async Task FollowAsync(string followerId, string followingId, string followerUserName, string followingUserName)
    {
        // First, ensure the users exist. MERGE is efficient.
        await AddUserAsync(followerId, followerUserName); // Placeholder for username, should be fetched from a user service
        await AddUserAsync(followingId, followingUserName);

        // MERGE finds or creates the entire pattern (follower -> following).
        // It's the perfect Cypher equivalent of your coalesce query.
        var query = @"
            MATCH (follower:User {id: $followerId})
            MATCH (following:User {id: $followingId})
            MERGE (follower)-[r:FOLLOWS]->(following)
            ON CREATE SET r.createdAt = datetime()
        ";

        await using var session = _client.AsyncSession();
        await session.ExecuteWriteAsync(async tx =>
        {
            await tx.RunAsync(query, new { followerId, followingId });
        });
    }

    public async Task<bool> IsFollowingAsync(string followerId, string followingId)
    {
        // EXISTS() is the most efficient way to check for a relationship.
        var query = @"
            MATCH (follower:User {id: $followerId})
            MATCH (following:User {id: $followingId})
            RETURN EXISTS((follower)-[:FOLLOWS]->(following))
        ";

        await using var session = _client.AsyncSession();
        return await session.ExecuteReadAsync(async tx =>
        {
            var result = await tx.RunAsync(query, new { followerId, followingId });
            return await result.SingleAsync(record => record[0].As<bool>());
        });
    }

    public async Task<bool> UnfollowAsync(string followerId, string followingId)
    {
        // This query finds the specific relationship and deletes it.
        var query = @"
            MATCH (follower:User {id: $followerId})-[r:FOLLOWS]->(following:User {id: $followingId})
            DELETE r
        ";

        await using var session = _client.AsyncSession();
        // We can get the summary to see if anything was deleted.
        var summary = await session.ExecuteWriteAsync(async tx =>
        {
            var result = await tx.RunAsync(query, new { followerId, followingId });
            return await result.ConsumeAsync();
        });

        // The operation "succeeds" regardless, but we can check if a relationship was actually removed.
        return summary.Counters.RelationshipsDeleted > 0;
    }

    public async Task AddUserAsync(string userId, string username)
    {
        var query = @"
            MERGE (u:User {id: $userId})
            ON CREATE SET u.username = $username
        ";

        await using var session = _client.AsyncSession();
        await session.ExecuteWriteAsync(async tx =>
        {
            await tx.RunAsync(query, new { userId, username });
        });
    }

    private Follow MapToFollow(dynamic result, bool isFollower, string subjectUserId)
    {
        var dict = (IDictionary<object, object>)result;

        if (!dict.TryGetValue("edge", out var edgeObj) || !dict.TryGetValue(isFollower ? "follower" : "following", out var vertexObj))
        {
            return null;
        }

        var edgeProps = GetPropertyMap(edgeObj);
        var vertexProps = GetPropertyMap(vertexObj);

        UserSummary follower, following;

        if (isFollower)
        {
            follower = MapToUserSummary(vertexProps);
            following = new UserSummary { UserId = subjectUserId, Username = subjectUserId };
        }
        else
        {
            follower = new UserSummary { UserId = subjectUserId, Username = subjectUserId };
            following = MapToUserSummary(vertexProps);
        }

        DateTime createdAt = DateTime.MinValue;
        if (edgeProps != null && edgeProps.TryGetValue("createdAt", out var createdAtValue))
        {
            DateTime.TryParse(GetScalarValue(createdAtValue), out createdAt);
        }

        return new Follow
        {
            FollowerInfo = follower,
            FollowingInfo = following,
            CreatedAt = createdAt.ToUniversalTime()
        };
    }

    private UserSummary MapToUserSummary(IDictionary<object, object> properties)
    {
        if (properties == null) return new UserSummary();

        return new UserSummary
        {
            UserId = GetScalarValue(properties["id"]),
            Username = GetScalarValue(properties["username"])
        };
    }

    private IDictionary<object, object> GetPropertyMap(object dynamicObject) => (IDictionary<object, object>)dynamicObject;

    private string GetScalarValue(object propertyValue)
    {
        if (propertyValue is List<object> list && list.Count > 0)
        {
            return list[0]?.ToString();
        }

        if (propertyValue is object[] array && array.Length > 0)
        {
            return array[0]?.ToString();
        }

        return propertyValue?.ToString();
    }

    public async Task<IEnumerable<UserSummaryDto>> GetFollowersAsUserSummariesAsync(string userId)
    {
        var query = @"
            MATCH (follower:User)-[:FOLLOWS]->(target:User {id: $userId})
            RETURN follower.id AS userId, 
                   follower.username AS username, 
                   follower.profilePictureUrl AS profilePictureUrl
        ";

        await using var session = _client.AsyncSession();
        return await session.ExecuteReadAsync(async tx =>
        {
            var result = await tx.RunAsync(query, new { userId });
            // The Neo4j driver has excellent mapping capabilities.
            return await result.ToListAsync(record => new UserSummaryDto
            {
                UserId = record["userId"].As<string>(),
                Username = record["username"].As<string>(),
                ProfilePictureUrl = record["profilePictureUrl"]?.As<string>()
            });
        });
    }

    public async Task<IEnumerable<UserSummaryDto>> GetFollowingAsUserSummariesAsync(string userId)
    {
        // The query is identical to GetFollowers, but the arrow direction is flipped.
        var query = @"
            MATCH (target:User {id: $userId})-[:FOLLOWS]->(following:User)
            RETURN following.id AS userId, 
                   following.username AS username, 
                   following.profilePictureUrl AS profilePictureUrl
        ";

        await using var session = _client.AsyncSession();
        return await session.ExecuteReadAsync(async tx =>
        {
            var result = await tx.RunAsync(query, new { userId });
            return await result.ToListAsync(record => new UserSummaryDto
            {
                UserId = record["userId"].As<string>(),
                Username = record["username"].As<string>(),
                ProfilePictureUrl = record["profilePictureUrl"]?.As<string>()
            });
        });
    }
    
        public async Task<IEnumerable<Follow>> GetFollowersAsync(string userId)
    {
        var query = @"
            MATCH (follower:User)-[r:FOLLOWS]->(target:User {id: $userId})
            RETURN follower, r.createdAt AS createdAt
        ";

        await using var session = _client.AsyncSession();
        return await session.ExecuteReadAsync(async tx =>
        {
            var result = await tx.RunAsync(query, new { userId });
            return await result.ToListAsync(record =>
            {
                var followerNode = record["follower"].As<INode>();
                var createdAt = record["createdAt"].As<ZonedDateTime>().ToDateTimeOffset().UtcDateTime;

                var followerInfo = new UserSummary
                {
                    UserId = followerNode.Properties["id"].As<string>(),
                    Username = followerNode.Properties["username"].As<string>()
                };

                // The 'following' user is the one we matched on
                var followingInfo = new UserSummary { UserId = userId, Username = userId };

                return new Follow { FollowerInfo = followerInfo, FollowingInfo = followingInfo, CreatedAt = createdAt };
            });
        });
    }

    public async Task<IEnumerable<Follow>> GetFollowingAsync(string userId)
    {
        var query = @"
            MATCH (target:User {id: $userId})-[r:FOLLOWS]->(following:User)
            RETURN following, r.createdAt AS createdAt
        ";

        await using var session = _client.AsyncSession();
        return await session.ExecuteReadAsync(async tx =>
        {
            var result = await tx.RunAsync(query, new { userId });
            return await result.ToListAsync(record =>
            {
                var followingNode = record["following"].As<INode>();
                var createdAt = record["createdAt"].As<ZonedDateTime>().ToDateTimeOffset().UtcDateTime;

                // The 'follower' user is the one we matched on
                var followerInfo = new UserSummary { UserId = userId, Username = userId };

                var followingInfo = new UserSummary
                {
                    UserId = followingNode.Properties["id"].As<string>(),
                    Username = followingNode.Properties["username"].As<string>()
                };
                
                return new Follow { FollowerInfo = followerInfo, FollowingInfo = followingInfo, CreatedAt = createdAt };
            });
        });
    }
}
