using MediatR;
using social_media9.Api.Dtos;
using social_media9.Api.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace social_media9.Api.Queries.SearchUsers
{
    public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, IEnumerable<UserSearchResultDto>>
    {
        private readonly IUserRepository _userRepository;

        public SearchUsersQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserSearchResultDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
        {
            var results = await _userRepository.SearchUsersAsync(request.Query, request.Limit);
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