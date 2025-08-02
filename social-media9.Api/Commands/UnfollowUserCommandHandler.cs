using MediatR;
using social_media9.Api.Data;
using social_media9.Api.Models;
using social_media9.Api.Repositories.Interfaces;

namespace social_media9.Api.Commands
{
    public class UnfollowUserCommandHandler : IRequestHandler<UnfollowUserCommand, Unit>
    {
        private readonly IFollowRepository _followRepository;
        private readonly IUserRepository _userRepository;

        public UnfollowUserCommandHandler(IFollowRepository followRepository, IUserRepository userRepository)
        {
            _followRepository = followRepository;
            _userRepository = userRepository;
        }

        public async Task<Unit> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
        {
            if (request.FollowerId == request.FollowingId)
            {
                throw new ApplicationException("Cannot unfollow yourself.");
            }

            var followingUserExists = await _userRepository.ExistsAsync(request.FollowingId);
            if (!followingUserExists)
            {
                throw new ApplicationException("User to unfollow not found.");
            }

            var isAlreadyFollowing = await _followRepository.IsFollowingAsync(request.FollowerId, request.FollowingId);
            if (!isAlreadyFollowing)
            {
                throw new ApplicationException("Not currently following this user.");
            }

            await _followRepository.RemoveFollowAsync(request.FollowerId, request.FollowingId);

            return Unit.Value;
        }
    }
}