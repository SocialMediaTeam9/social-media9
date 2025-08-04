using MediatR;
using social_media9.Api.Models;
using social_media9.Api.Data;
using social_media9.Api;
using social_media9.Api.Repositories.Interfaces;

namespace social_media9.Api.Commands
{
    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UserProfile>
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserProfileCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserProfile> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            User ? user = (await _userRepository.GetUsersByIdsAsync(new[] { request.UserId })).FirstOrDefault();

            if (user == null)
            {
                throw new ApplicationException("User not found.");
            }

            // Update only provided fields
            user.FullName = request.FullName ?? user.FullName;
            user.Bio = request.Bio ?? user.Bio;
            user.ProfilePictureUrl = request.ProfilePictureUrl ?? user.ProfilePictureUrl;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateUserAsync(user);

            return new UserProfile
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Bio = user.Bio,
                ProfilePictureUrl = user.ProfilePictureUrl,
                FollowersCount = user.FollowersCount,
                FollowingCount = user.FollowingCount,
                CreatedAt = user.CreatedAt,
                GoogleId = user.GoogleId
            };
        }
    }
}