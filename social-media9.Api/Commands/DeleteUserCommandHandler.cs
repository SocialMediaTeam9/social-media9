using MediatR;
// using social_media9.Api.Data;
using social_media9.Api.Dtos;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Services.Interfaces;
using social_media9.Api.Models;
using System; // For ApplicationException
using System.Threading; // For CancellationToken
using System.Threading.Tasks;

namespace social_media9.Api.Commands
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
    {
        private readonly IUserRepository _userRepository;
        private readonly IFollowRepository _followRepository; // To clean up follow relationships

        public DeleteUserCommandHandler(IUserRepository userRepository, IFollowRepository followRepository)
        {
            _userRepository = userRepository;
            _followRepository = followRepository;
        }

        public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var userExists = await _userRepository.ExistsAsync(request.UserId);
            if (!userExists)
            {
                throw new ApplicationException("User not found.");
            }

            // Delete user
            await _userRepository.DeleteUserAsync(request.UserId);

            // OPTIONAL: Clean up follow relationships where this user is involved
            // This can be complex and might be better handled by a background process
            // For simplicity, we'll just demonstrate deleting entries directly related to this user
            // In a real system, you'd likely have cascade deletes or more robust cleanup
            // var followers = await _followRepository.GetFollowersAsync(request.UserId);
            // foreach (var followerId in followers)
            // {
            //     await _followRepository.RemoveFollowAsync(followerId, request.UserId);
            // }

            // var following = await _followRepository.GetFollowingAsync(request.UserId);

            // foreach (var followingId in following)
            // {
            //     await _followRepository.RemoveFollowAsync(request.UserId, followingId);
            // }

            return Unit.Value;
        }
    }
}