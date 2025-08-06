using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nest;
using social_media9.Api.Configurations;
using social_media9.Api.Models;
using social_media9.Api.Repositories.Interfaces;

namespace social_media9.Api.Repositories.Implementations
{
    public class ElasticsearchRepository : ISearchRepository
    {
        private readonly IElasticClient _client;
        private readonly ElasticsearchSettings _settings;

        public ElasticsearchRepository(IElasticClient client, ElasticsearchSettings settings)
        {
            _client = client;
            _settings = settings;
        }

        public async Task<IEnumerable<UserSearchDocument>> SearchUsersAsync(string query, int limit, CancellationToken cancellationToken)
        {
            var response = await _client.SearchAsync<UserSearchDocument>(s => s
                .Index(_settings.UsersIndex)
                .Size(limit)
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Query(query)
                        .Fields(f => f
                            .Field(u => u.Username, boost: 2) // Boost username matches
                            .Field(u => u.FullName)
                        )
                        .Fuzziness(Fuzziness.Auto) // For typo tolerance
                    )
                ), cancellationToken
            );
            
            if (!response.IsValid) throw new System.Exception("Elasticsearch user query failed", response.OriginalException);

            return response.Documents;
        }

        public async Task<IEnumerable<PostSearchDocument>> SearchPostsAsync(string query, int limit, CancellationToken cancellationToken)
        {
            var response = await _client.SearchAsync<PostSearchDocument>(s => s
                .Index(_settings.PostsIndex)
                .Size(limit)
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Query(query)
                        .Fields(f => f.Field(p => p.Content))
                    )
                )
                .Sort(so => so.Descending(f => f.CreatedAt)), // Show newest posts first
                cancellationToken
            );
            
            if (!response.IsValid) throw new System.Exception("Elasticsearch content query failed", response.OriginalException);
            
            return response.Documents;
        }

        public async Task<IEnumerable<PostSearchDocument>> SearchHashtagsAsync(string tag, int limit, CancellationToken cancellationToken)
        {
            // Ensure tag is clean (no '#') and lowercase for consistent matching
            var cleanTag = tag.TrimStart('#').ToLowerInvariant();

            var response = await _client.SearchAsync<PostSearchDocument>(s => s
                .Index(_settings.PostsIndex)
                .Size(limit)
                .Query(q => q
                    .Term(t => t
                        .Field(p => p.Hashtags)
                        .Value(cleanTag)
                    )
                )
                .Sort(so => so.Descending(f => f.CreatedAt)),
                cancellationToken
            );

            if (!response.IsValid) throw new System.Exception("Elasticsearch hashtag query failed", response.OriginalException);

            return response.Documents;
        }
    }
}