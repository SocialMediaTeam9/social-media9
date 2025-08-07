using MediatR;
using social_media9.Api.Data;
using social_media9.Api.Models;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Services.DynamoDB;

namespace social_media9.Api.Commands
{
    public class UnfollowUserCommandHandler : IRequestHandler<UnfollowUserCommand, Unit>
    {
        private readonly IFollowRepository _followRepository;
        private readonly IUserRepository _userRepository;
        private readonly FollowService _followService;
        private readonly IConfiguration _config;


        public UnfollowUserCommandHandler(IFollowRepository followRepository, IUserRepository userRepository, FollowService followService, IConfiguration config)
        {
            _followRepository = followRepository;
            _userRepository = userRepository;
            _followService = followService;
            _config = config;
        }

        public async Task<Unit> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
        {

            var follower = await _userRepository.GetUserByIdAsync(request.FollowerId);

            if (follower == null)
            {
                throw new ApplicationException("Current user not found.");
            }

            var domain = _config["DomainName"];
            var selfActorUrl = $"https://{domain}/users/{follower.Username}";
            if (selfActorUrl == request.UnfollowedActorUrl)
            {
                throw new ApplicationException("Cannot unfollow yourself.");
            }

            // if (follower == null || following == null)
            // {
            //     throw new ApplicationException("User not found.");
            // }

            // if (!followingUserExists)
            // {
            //     throw new ApplicationException("User to unfollow not found.");
            // }

            // var isAlreadyFollowing = await _followRepository.IsFollowingAsync(request.FollowerId, request.FollowingId);
            // if (!isAlreadyFollowing)
            // {
            //     throw new ApplicationException("Not currently following this user.");
            // }

            var success = await _followService.UnfollowUserAsync(follower.Username, request.UnfollowedActorUrl);

            if (!success)
            {

                throw new ApplicationException("Could not unfollow user. You may not be following them.");
            }


            return Unit.Value;
        }
    }
}