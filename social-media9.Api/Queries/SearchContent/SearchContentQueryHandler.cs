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
        private readonly IPostRepository _postRepository;

        public SearchContentQueryHandler(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public async Task<IEnumerable<PostSearchResultDto>> Handle(SearchContentQuery request, CancellationToken cancellationToken)
        {
            var results = await _postRepository.SearchPostsAsync(request.Query, request.Limit);

            return results.Select(post => new PostSearchResultDto
            {

                PostId = post.SK.Replace("POST#", ""),
                UserId = post.PK.Replace("USER#", ""),
                Username = post.AuthorUsername,
                Content = post.Content,
                Hashtags = ExtractHashtags(post.Content),
                CreatedAt = post.CreatedAt
            });
        }
        
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