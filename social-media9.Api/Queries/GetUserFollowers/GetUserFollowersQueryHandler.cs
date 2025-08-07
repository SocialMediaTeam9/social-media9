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
    public class GetUserFollowersQueryHandler : IRequestHandler<GetUserFollowersQuery, IEnumerable<UserSummary>>
    {
        private readonly IFollowRepository _followRepository;
        private readonly IUserRepository _userRepository;

        public GetUserFollowersQueryHandler(IFollowRepository followRepository, IUserRepository userRepository)
        {
            _followRepository = followRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserSummary>> Handle(GetUserFollowersQuery request, CancellationToken cancellationToken)
        {
            var userExists = await _userRepository.ExistsAsync(request.Username);
            if (!userExists)
            {
                throw new ApplicationException("User not found.");
            }

            var followerEntities = await _followRepository.GetFollowersAsync(request.Username);

            var summaries = followerEntities.Select(entity => entity.FollowerInfo);
            
            // var users = await _userRepository.GetUsersByIdsAsync(followerIds);

            // var summaries = users.Select(user => new UserSummary
            // {
            //     UserId = user.UserId,
            //     Username = user.Username,
            //     ProfilePictureUrl = user.ProfilePicture
            // });

            return summaries;
        }
    }
}
