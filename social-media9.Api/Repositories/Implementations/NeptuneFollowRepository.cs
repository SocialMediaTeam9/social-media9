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
         var query = $@"
        g.V().has('user', 'id', '{followerId}')
          .as('follower')
          .V().has('user', 'id', '{followingId}')
          .coalesce(
              inE('follows').where(outV().as('follower')),
              addE('follows').from('follower').property('createdAt', '{DateTime.UtcNow:o}')
          )
    ";

    await _client.SubmitAsync<dynamic>(query);
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
        throw new NotImplementedException();
    }
}
