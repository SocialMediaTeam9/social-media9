using MediatR;
using social_media9.Api.Models;
using social_media9.Api.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using social_media9.Api.Repositories.Interfaces;

namespace social_media9.Api
{
    public class GetUserFollowingQueryHandler : IRequestHandler<GetUserFollowingQuery, IEnumerable<UserSummary>>
    {
        private readonly IFollowRepository _followRepository;
        private readonly IUserRepository _userRepository;

        public GetUserFollowingQueryHandler(IFollowRepository followRepository, IUserRepository userRepository)
        {
            _followRepository = followRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserSummary>> Handle(GetUserFollowingQuery request, CancellationToken cancellationToken)
        {
            var userExists = await _userRepository.ExistsAsync(request.UserId);
            if (!userExists)
            {
                throw new ApplicationException("User not found.");
            }

            var followingIds = await _followRepository.GetFollowingAsync(request.UserId);
            if (!followingIds.Any())
            {
                return Enumerable.Empty<UserSummary>();
            }

            var users = await _userRepository.GetUsersByIdsAsync(followingIds);

            var summaries = users.Select(user => new UserSummary
            {
                UserId = user.UserId,
                Username = user.Username,
                ProfilePictureUrl = user.ProfilePicture
            });

            return summaries;
        }
    }
}
