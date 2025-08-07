using MediatR;
using social_media9.Api.Models;
using social_media9.Api.Data;
using social_media9.Api.Repositories.Interfaces;

namespace social_media9.Api
{
    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfile?>
    {
        private readonly IUserRepository _userRepository;

        public GetUserProfileQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserProfile?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            // User? user = (await _userRepository.GetUsersByIdsAsync(new[] { request.UserId })).FirstOrDefault();

            User? user = await _userRepository.GetUserByIdAsync(request.UserId);

            if (user == null)
            {
                return null; // User not found
            }

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