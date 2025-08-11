using Gremlin.Net.Driver;
using Gremlin.Net.Structure;
using Gremlin.Net.Structure.IO.GraphSON;
using Gremlin.Net.Process.Traversal;
using social_media9.Api.Models;
using social_media9.Api.Repositories.Interfaces;

public class NeptuneFollowRepository : IFollowRepository
{
    private readonly IGremlinClient _client;

    public NeptuneFollowRepository(IGremlinClient gremlinClient)
    {
        _client = gremlinClient ?? throw new ArgumentNullException(nameof(gremlinClient));
    }

    public async Task FollowAsync(string followerId, string followingId)
    {
        await AddUserAsync(followerId, followerId); //this has to be changed to the actual username!!!!!
        await AddUserAsync(followingId, followingId);

        var query = @"g.V().has('user', 'id', p_followerId).as('follower')
                       .V().has('user', 'id', p_followingId).as('following')
                       .coalesce(__.inE('follows').where(outV().as('follower')),
                                 addE('follows').from('follower').to('following').property('createdAt', p_createdAt))";

        var bindings = new Dictionary<string, object>
        {
            { "p_followerId", followerId },
            { "p_followingId", followingId },
            { "p_createdAt", DateTime.UtcNow }
        };

        await _client.SubmitAsync<dynamic>(query, bindings);
    }

    public async Task<IEnumerable<Follow>> GetFollowersAsync(string userId)
    {
        var query = @"g.V().has('user', 'id', p_userId)
                         .inE('follows').as('edge')
                         .outV().as('follower')
                         .project('follower', 'edge')
                         .by(valueMap(true))
                         .by(valueMap(true))";

        var bindings = new Dictionary<string, object> { { "p_userId", userId } };

        var results = await _client.SubmitAsync<dynamic>(query, bindings);

        if (results == null) return Enumerable.Empty<Follow>();

        
        var follows = new List<Follow>();

        foreach (var result in results)
        {
            var follow = MapToFollow(result, isFollower: true, subjectUserId: userId);
            if (follow != null)
            {
                follows.Add(follow);
            }
        }

        return follows;
    }

    public async Task<IEnumerable<Follow>> GetFollowingAsync(string userId)
    {
        var query = $@"
            g.V().has('user','id','{userId}')
                 .outE('follows').as('edge')
                 .inV().as('following')
                 .select('following','edge')
        ";

        var results = await _client.SubmitAsync<dynamic>(query);

        var follows = new List<Follow>();
        foreach (var r in results)
        {
            var followingProps = r["following"];
            var edgeProps = r["edge"];

            var followerSummary = new UserSummary
            {
                UserId = userId,
                Username = "" // You could hydrate this from a user repo if needed
            };

            var followingSummary = new UserSummary
            {
                UserId = followingProps["id"]?.ToString() ?? string.Empty,
                Username = followingProps["username"]?.ToString() ?? string.Empty
            };

            follows.Add(new Follow
            {
                FollowerInfo = followerSummary,
                FollowingInfo = followingSummary,
                CreatedAt = DateTime.TryParse(edgeProps["createdAt"]?.ToString(), out var dt) ? dt : DateTime.UtcNow
            });
        }

        return follows;
    }

    public Task<bool> IsFollowingAsync(string followerId, string followingId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UnfollowAsync(string followerId, string followingId)
    {
        var query = @"g.V().has('user', 'id', p_followerId)
                       .outE('follows').where(__.inV().has('user', 'id', p_followingId))
                       .drop()";

        var bindings = new Dictionary<string, object>
        {
            { "p_followerId", followerId },
            { "p_followingId", followingId }
        };

        await _client.SubmitAsync<dynamic>(query, bindings);
        return true;
    }

    public async Task AddUserAsync(string userId, string username)
    {
        var query = @"g.V().has('user', 'id', p_userId).fold()
                       .coalesce(unfold(),
                                 addV('user').property('id', p_userId).property('username', p_username))";

        var bindings = new Dictionary<string, object>
        {
            { "p_userId", userId },
            { "p_username", username }
        };

        await _client.SubmitAsync<dynamic>(query, bindings);
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
}
