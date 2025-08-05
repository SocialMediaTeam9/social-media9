using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using social_media9.Api.Dtos;
using social_media9.Api.Repositories.Interfaces;

namespace social_media9.Api.Queries.SearchContent
{
    public class SearchContentQueryHandler : IRequestHandler<SearchContentQuery, IEnumerable<PostSearchResultDto>>
    {
        private readonly ISearchRepository _repository;

        public SearchContentQueryHandler(ISearchRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<PostSearchResultDto>> Handle(SearchContentQuery request, CancellationToken cancellationToken)
        {
            var results = await _repository.SearchPostsAsync(request.Query, request.Limit, cancellationToken);

            return results.Select(post => new PostSearchResultDto
            {
                PostId = post.PostId,
                UserId = post.UserId,
                Username = post.Username,
                Content = post.Content,
                Hashtags = post.Hashtags,
                CreatedAt = post.CreatedAt
            });
        }
    }
}