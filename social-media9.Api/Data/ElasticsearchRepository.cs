using MediatR;
using Nest;
using social_media9.Api.Models;

namespace social_media9.Api.Data;

public class ElasticsearchRepository : ISearchRepository
{
  private readonly IElasticClient _client;
  // Define constants for your index names
  private const string ContentIndex = "content";
  private const string UsersIndex = "users";
  private const string NotificationsIndex = "notifications";

  public ElasticsearchRepository(IElasticClient client)
  {
    _client = client;
  }

  public async Task<IEnumerable<UserSummary>> SearchUsersAsync(string query, int limit, CancellationToken cancellationToken)
  {
    var response = await _client.SearchAsync<UserSummary>(s => s
        .Index(UsersIndex)
        .Size(limit)
        .Query(q => q
            .MultiMatch(mm => mm
                .Query(query)
                .Fields(f => f.Field(u => u.Username).Field(u => u.FullName))
                .Fuzziness(Fuzziness.Auto) // For typo tolerance
            )
        ), cancellationToken
    );
    return response.Documents;
  }

  public async Task<IEnumerable<VideoPostSummary>> SearchContentAsync(string query, int limit, CancellationToken cancellationToken)
  {
    var response = await _client.SearchAsync<VideoPostSummary>(s => s
        .Index(ContentIndex)
        .Size(limit)
        .Query(q => q
            .MultiMatch(mm => mm
                .Query(query)
                .Fields(f => f.Field(p => p.Title).Field(p => p.Description))
            )
        ), cancellationToken
    );
    return response.Documents;
  }

  public async Task<IEnumerable<VideoPostSummary>> SearchHashtagsAsync(string tag, int limit, CancellationToken cancellationToken)
  {
    // Ensure tag is clean (no '#') and lowercase for consistent matching
    var cleanTag = tag.TrimStart('#').ToLowerInvariant();
    var response = await _client.SearchAsync<VideoPostSummary>(s => s
        .Index(ContentIndex)
        .Size(limit)
        .Query(q => q
            .Term(t => t
                .Field(p => p.Hashtags) // Assumes hashtags are indexed as 'keyword' type
                .Value(cleanTag)
            )
        ), cancellationToken
    );
    return response.Documents;
  }

  public async Task<IEnumerable<Notification>> GetNotificationsAsync(string userId, int limit, CancellationToken cancellationToken)
  {
    var response = await _client.SearchAsync<Notification>(s => s
        .Index(NotificationsIndex)
        .Size(limit)
        .Query(q => q
            .Term(t => t
                .Field(n => n.UserId)
                .Value(userId)
            )
        )
        .Sort(so => so.Descending(f => f.CreatedAt)), // Show newest notifications first
        cancellationToken
    );
    return response.Documents;
  }
}