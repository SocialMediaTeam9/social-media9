using MediatR;
using social_media9.Api.Models;
using social_media9.Api.Data;
using social_media9.Api.Services;
using social_media9.Api.Services.Interfaces;
using social_media9.Api.Dtos;
using social_media9.Api.Repositories.Interfaces;
using System;

namespace social_media9.Api.Commands
{
    public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, AuthResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IGoogleAuthService _googleAuthService;

        public GoogleLoginCommandHandler(IUserRepository userRepository, IJwtGenerator jwtGenerator, IGoogleAuthService googleAuthService)
        {
            _userRepository = userRepository;
            _jwtGenerator = jwtGenerator;
            _googleAuthService = googleAuthService;
        }

        public async Task<AuthResponseDto> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
        {
            // 1. Exchange authorization code for access token
            var tokenResponse = await _googleAuthService.ExchangeCodeForTokenAsync(request.Code, request.RedirectUri);
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                throw new ApplicationException("Failed to exchange Google authorization code for token.");
            }

            // 2. Get user info from Google using the access token
            var googleUserInfo = await _googleAuthService.GetUserInfoAsync(tokenResponse.AccessToken);
            if (googleUserInfo == null || string.IsNullOrEmpty(googleUserInfo.Id))
            {
                throw new ApplicationException("Failed to retrieve user info from Google.");
            }

            // 3. Find or create user in your database
            var user = await _userRepository.GetUserByGoogleIdAsync(googleUserInfo.Id);

            if (user == null)
            {
                // New user - create an entry
                user = new User
                {
                    UserId = Guid.NewGuid().ToString(), // Generate a new internal UUID
                    GoogleId = googleUserInfo.Id,
                    Username = googleUserInfo.Email.Split('@')[0], // Default username from email
                    Email = googleUserInfo.Email,
                    FullName = googleUserInfo.Name,
                    ProfilePictureUrl = googleUserInfo.Picture,
                    CreatedAt = DateTime.UtcNow
                };
                await _userRepository.AddUserAsync(user);
            }
            else
            {
                // Existing user - update their details if necessary (e.g., profile picture, name)
                user.Email = googleUserInfo.Email; // Keep email updated
                user.FullName = googleUserInfo.Name;
                user.ProfilePictureUrl = googleUserInfo.Picture;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
            }

            // 4. Generate internal JWT for your application
            var internalJwt = _jwtGenerator.GenerateToken(user.UserId, user.Username);

            return new AuthResponseDto { UserId = user.UserId, Username = user.Username, Token = internalJwt };
        }
    }
}
