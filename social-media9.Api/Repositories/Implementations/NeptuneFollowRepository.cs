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

    public Task<IEnumerable<Follow>> GetFollowersAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Follow>> GetFollowingAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsFollowingAsync(string followerId, string followingId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UnfollowAsync(string followerId, string followingId)
    {
         var checkQuery = $@"
            g.V().has('user','id','{followerId}')
            .outE('follows')
            .where(inV().has('user','id','{followingId}'))
            .count()
        ";
        var countResult = await _client.SubmitAsync<long>(checkQuery);
        var exists = countResult.FirstOrDefault() > 0;

        if (exists)
        {
            var dropQuery = $@"
                g.V().has('user','id','{followerId}')
                .outE('follows')
                .where(inV().has('user','id','{followingId}'))
                .drop()
            ";
            await _client.SubmitAsync<dynamic>(dropQuery);
        }

        return exists;
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
}
