// File: Api/Queries/SearchContent/SearchContentQueryHandler.cs
// <<< THIS FILE IS UPDATED >>>

using MediatR;
using social_media9.Api.Dtos;
using social_media9.Api.Repositories.Interfaces; // Using the main repository interface
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace social_media9.Api.Queries.SearchContent
{
    public class SearchContentQueryHandler : IRequestHandler<SearchContentQuery, IEnumerable<PostSearchResultDto>>
    {
        // <<< CHANGE: Inject IPostRepository instead of ISearchRepository >>>
        private readonly IPostRepository _postRepository;

        public SearchContentQueryHandler(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public async Task<IEnumerable<PostSearchResultDto>> Handle(SearchContentQuery request, CancellationToken cancellationToken)
        {
            // <<< CHANGE: Call the new search method on the post repository >>>
            var results = await _postRepository.SearchPostsAsync(request.Query, request.Limit);

            // The mapping logic is updated to handle the DynamoDB 'Post' model.
            return results.Select(post => new PostSearchResultDto
            {
                // Assuming Post model has these properties. Adjust if necessary.
                PostId = post.SK.Replace("POST#", ""), // Get ID from Sort Key
                UserId = post.PK.Replace("USER#", ""), // Get ID from Partition Key
                Username = post.AuthorUsername,
                Content = post.Content,
                Hashtags = ExtractHashtags(post.Content), // Extract hashtags from content
                CreatedAt = post.CreatedAt
            });
        }
        
        // Helper function to extract hashtags from post content
        private List<string> ExtractHashtags(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return new List<string>();
            }
            return Regex.Matches(content, @"#(\w+)")
                        .Cast<Match>()
                        .Select(m => m.Groups[1].Value)
                        .Distinct()
                        .ToList();
        }
    }
}