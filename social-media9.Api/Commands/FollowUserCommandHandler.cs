using MediatR;
using social_media9.Api.Data;
using System; // For ApplicationException
using System.Threading; // For CancellationToken
using System.Threading.Tasks; 
using social_media9.Api.Models;

namespace social_media9.Api.Commands
{
    public class FollowUserCommandHandler : IRequestHandler<FollowUserCommand, Unit>
    {
        private readonly IFollowRepository _followRepository;
        private readonly IUserRepository _userRepository;

        public FollowUserCommandHandler(IFollowRepository followRepository, IUserRepository userRepository)
        {
            _followRepository = followRepository;
            _userRepository = userRepository;
        }

        public async Task<Unit> Handle(FollowUserCommand request, CancellationToken cancellationToken)
        {
            if (request.FollowerId == request.FollowingId)
            {
                throw new ApplicationException("Cannot follow yourself.");
            }

            var followingUserExists = await _userRepository.ExistsAsync(request.FollowingId);
            if (!followingUserExists)
            {
                throw new ApplicationException("User to follow not found.");
            }

            var isAlreadyFollowing = await _followRepository.IsFollowingAsync(request.FollowerId, request.FollowingId);
            if (isAlreadyFollowing)
            {
                throw new ApplicationException("Already following this user.");
            }

            await _followRepository.AddFollowAsync(request.FollowerId, request.FollowingId);

            return Unit.Value;
        }
    }
}