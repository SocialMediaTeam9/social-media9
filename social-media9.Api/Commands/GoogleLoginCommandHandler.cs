using MediatR;
using social_media9.Api.Models;
using social_media9.Api.Data;
using social_media9.Api.Services;
using social_media9.Api.Services.Interfaces;
using social_media9.Api.Dtos;
using social_media9.Api.Repositories.Interfaces;
using System;
using social_media9.Api.Services.DynamoDB;

namespace social_media9.Api.Commands
{
    public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, AuthResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly DynamoDbService _dbService;
        private readonly ICryptoService _cryptoService;

        public GoogleLoginCommandHandler(IUserRepository userRepository, IJwtGenerator jwtGenerator, IGoogleAuthService googleAuthService, DynamoDbService dbService, ICryptoService cryptoService)
        {
            _userRepository = userRepository;
            _jwtGenerator = jwtGenerator;
            _googleAuthService = googleAuthService;
            _dbService = dbService;
            _cryptoService = cryptoService;
        }

        public async Task<AuthResponseDto> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
        {
            // 1. Exchange authorization code for access token
            var tokenResponse = await _googleAuthService.ExchangeCodeForTokenAsync(request.Code, request.RedirectUri);
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                throw new ApplicationException("Failed to exchange Google authorization code for token.");
            }

            var googleUserInfo = await _googleAuthService.GetUserInfoAsync(tokenResponse.AccessToken);
            if (googleUserInfo == null || string.IsNullOrEmpty(googleUserInfo.Id))
            {
                throw new ApplicationException("Failed to retrieve user info from Google.");
            }


            var user = await _userRepository.GetUserByGoogleIdAsync(googleUserInfo.Id);


            if (user == null)
            {
                (string publicKey, string privateKey) = _cryptoService.GenerateRsaKeyPair();

                var username = googleUserInfo.Email.Split('@')[0]; // Default username from email

                user = new User
                {
                    UserId = Guid.NewGuid().ToString(), // Generate a new internal UUID
                    PK = $"USER#{username}",
                    SK = "METADATA",
                    GSI1PK = $"USER#{username}",
                    GSI1SK = "METADATA",
                    GoogleId = googleUserInfo.Id,
                    Username = googleUserInfo.Email.Split('@')[0]  ?? Ulid.NewUlid().ToString(), 
                    Email = googleUserInfo.Email,
                    FullName = googleUserInfo.Name,
                    ProfilePictureUrl = googleUserInfo.Picture,
                    PublicKeyPem = publicKey,
                    PrivateKeyPem = privateKey,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbService.CreateUserAsync(user);

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
