using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using social_media9.Api.Dtos;
using social_media9.Api.Repositories.Interfaces;

namespace social_media9.Api.Queries.SearchUsers
{
    public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, IEnumerable<UserSearchResultDto>>
    {
        private readonly ISearchRepository _repository;

        public SearchUsersQueryHandler(ISearchRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<UserSearchResultDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
        {
            var results = await _repository.SearchUsersAsync(request.Query, request.Limit, cancellationToken);

            return results.Select(user => new UserSearchResultDto
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                ProfilePictureUrl = user.ProfilePictureUrl
            });
        }
    }
}