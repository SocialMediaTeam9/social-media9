using MediatR;
using FluentValidation;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Services.Interfaces;
using social_media9.Api.Services;
using Microsoft.AspNetCore.Mvc;
using social_media9.Api.Models;
using social_media9.Api.Commands;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using social_media9.Api.Services.DynamoDB;

namespace social_media9.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IUserRepository _userRepository;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IConfiguration _config;
        private readonly IS3StorageService _s3StorageService;
        private readonly FollowService _followService;
        private readonly ILogger<FederationController> _logger;
        private readonly DynamoDbService _dbService;
        private readonly ICryptoService _cryptoService;

        public UsersController(
            IMediator mediator,
            IUserRepository userRepository,
            IJwtGenerator jwtGenerator,
            IConfiguration config,
            IS3StorageService s3StorageService,
            FollowService followService, DynamoDbService dbService, ICryptoService cryptoService, ILogger<FederationController> logger)
        {
            _mediator = mediator;
            _userRepository = userRepository;
            _jwtGenerator = jwtGenerator;
            _config = config;
            _s3StorageService = s3StorageService;
            _followService = followService;
            _dbService = dbService;
            _cryptoService = cryptoService;
            _logger = logger;
        }

        private string GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }
            return userIdClaim;
        }

        

        [HttpGet("signin-google")]
        [AllowAnonymous]
        public IActionResult GoogleLogin()
        {
            var redirectUri = Url.Action(nameof(GoogleLoginCallback), "Users", null, Request.Scheme);
            var properties = new AuthenticationProperties { RedirectUri = redirectUri };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLoginCallback()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded || result.Principal == null)
                return BadRequest("Google authentication failed.");

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;
            var googleId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(googleId))
                return BadRequest("Google ID not found.");

            var user = await _userRepository.GetUserByGoogleIdAsync(googleId);

            if (user == null)
            {
                (string publicKey, string privateKey) = _cryptoService.GenerateRsaKeyPair();
                var username = email?.Split('@')[0]  ?? Ulid.NewUlid().ToString();
                user = new User
                {
                    UserId = Guid.NewGuid().ToString(),
                    PK = $"USER#{username}",
                    SK = "METADATA",
                    GSI1PK = $"USER#{username}",
                    GSI1SK = "METADATA",
                    GoogleId = googleId,
                    Username = username,
                    Email = email ?? "",
                    FullName = name ?? "",
                    ProfilePictureUrl = result.Principal.FindFirst("picture")?.Value ?? "",
                    PublicKeyPem = publicKey,
                    PrivateKeyPem = privateKey,
                    CreatedAt = DateTime.UtcNow,
                };
                await _dbService.CreateUserAsync(user);
                // await _userRepository.AddUserAsync(user);
            }

            var jwt = _jwtGenerator.GenerateToken(user.UserId, user.Username);
            var frontendRedirect = _config["GoogleAuthSettings:FrontendRedirectUri"];
            var redirectUrl = $"{frontendRedirect}?userId={user.UserId}&username={user.Username}&token={jwt}";

            return Redirect(redirectUrl);
        }

        [HttpPost("google-login")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                var command = new GoogleLoginCommand
                {
                    Code = request.Code,
                    RedirectUri = request.RedirectUri
                };
                var response = await _mediator.Send(command);
                return Ok(response);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during Google login.", error = ex.Message });
            }
        }

       

        [HttpGet("{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile(string userId)
        {
            try
            {
                var query = new GetUserProfileQuery { UserId = userId };
                var userProfile = await _mediator.Send(query);
                return userProfile != null ? Ok(userProfile) : NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving profile.");
                return StatusCode(500, new { message = "Error retrieving profile." });
            }
        }

        [HttpPut("{userId}")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile(string userId, [FromBody] UserProfileUpdate request)
        {
            try
            {
                if (GetCurrentUserId() != userId)
                    return Forbid();

                var command = new UpdateUserProfileCommand
                {
                    UserId = userId,
                    FullName = request.FullName,
                    Bio = request.Bio,
                    ProfilePictureUrl = request.ProfilePictureUrl
                };
                var updated = await _mediator.Send(command);
                return Ok(updated);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (ApplicationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { message = "Error updating profile." });
            }
        }

     

        [HttpPost("{userId}/follow")]
        [Authorize]
        public async Task<IActionResult> FollowUser(string userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var command = new FollowUserCommand { FollowerId = currentUserId, FollowingId = userId };
                await _mediator.Send(command);
                return Ok(new { message = "User followed." });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { message = "Error following user." });
            }
        }

        [HttpDelete("{userId}/unfollow")]
        [Authorize]
        public async Task<IActionResult> UnfollowUser([FromBody] UnfollowUserRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var command = new UnfollowUserCommand { FollowerId = currentUserId, UnfollowedActorUrl = request.ActorUrl };
                await _mediator.Send(command);
                
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { message = "Error unfollowing user." });
            }
        }

        

        // [HttpPost("followers")]
        // public async Task<IActionResult> GetFollowers([FromBody] GtsCollectionRequest request)
        // {
        //     var query = new GetUserFollowersQuery { Username = request.Username };
        //     var followers = await _mediator.Send(query);
        //     return Ok(followers);
        //     // var response = new GtsCollectionResponse(followerUrls);
        //     // return Ok(response);
        // }

        // [HttpPost("following")]
        // public async Task<IActionResult> GetFollowing([FromBody] GtsCollectionRequest request)
        // {
        //     var query = new GetUserFollowingQuery { Username = request.Username };
        //     var following = await _mediator.Send(query);
        //     return Ok(following);
        //     // var followingEntities = await _dbService.GetFollowingAsync(request.Username);
        //     // var followingUrls = followingEntities
        //     //     .Select(entity => entity.FollowingInfo.ActorUrl)
        //     //     .ToList();

        //     // var response = new GtsCollectionResponse(followingUrls);
        //     // return Ok(response);
        // }

        [HttpGet("{username}/followers")]
        public async Task<IActionResult> GetUserFollowers(string username)
        {
            try
            {
                var query = new GetUserFollowersQuery { Username = username };
                var followers = await _mediator.Send(query);
                return Ok(followers);
            }
            catch (ApplicationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { message = "Error retrieving followers." });
            }
        }

        [HttpGet("{username}/following")]
        public async Task<IActionResult> GetUserFollowing(string username)
        {
            try
            {
                var query = new GetUserFollowingQuery { Username = username };
                var following = await _mediator.Send(query);
                return Ok(following);
            }
            catch (ApplicationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { message = "Error retrieving following list." });
            }
        }


        [HttpPost("{userId}/profile-picture")]
        [Authorize]
        public async Task<IActionResult> UploadProfilePicture(string userId, IFormFile file)
        {
            try
            {
                if (GetCurrentUserId() != userId)
                    return Forbid();

                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "No file uploaded." });

                if (!file.ContentType.StartsWith("image/"))
                    return BadRequest(new { message = "Only image files are allowed." });

                // Generate a unique file name
                var fileName = $"{userId}-{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

                // Upload to S3
                string fileUrl;
                using (var stream = file.OpenReadStream())
                {
                    fileUrl = await _s3StorageService.UploadFileAsync(stream, fileName, file.ContentType);
                }

                // Now, update the user's ProfilePictureUrl in your database
                // You'll likely need a new MediatR command for this or modify UpdateUserProfileCommand
                var updateCommand = new UpdateUserProfileCommand // Reusing or creating a new command
                {
                    UserId = userId,
                    ProfilePictureUrl = fileUrl,
                    // You might want to retrieve existing FullName and Bio if this is a dedicated update
                    // or modify your frontend to send only the changed fields.
                    // For simplicity, let's assume we are only updating the picture here.
                };

                // Fetch current user details to preserve other fields if only updating picture
                var currentUser = await _userRepository.GetUserByIdAsync(userId);
                if (currentUser == null) return NotFound();

                updateCommand.FullName = currentUser.FullName; // Preserve
                updateCommand.Bio = currentUser.Bio; // Preserve

                var updatedUser = await _mediator.Send(updateCommand);

                return Ok(new { profilePictureUrl = fileUrl, message = "Profile picture updated successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error uploading profile picture.", error = ex.Message });
            }
        }

    }
}
