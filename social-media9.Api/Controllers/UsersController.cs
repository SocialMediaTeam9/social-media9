using MediatR;
using FluentValidation;
using social_media9.Api.Data;
using social_media9.Api.Services;
using Microsoft.AspNetCore.Mvc;
using social_media9.Api.Models;
using social_media9.Api.Commands;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

namespace social_media9.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IUserRepository _userRepository;
        private readonly IJwtGenerator _jwtGenerator;

        public UsersController(IMediator mediator, IUserRepository userRepository, IJwtGenerator jwtGenerator)
        {
            _mediator = mediator;
            _userRepository = userRepository;
            _jwtGenerator = jwtGenerator;
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

        [HttpGet("google-login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public IActionResult GoogleLogin()
        {
            // This RedirectUri specifies where the user is sent *after* the middleware handles the callback.
            var redirectUri = Url.Action(nameof(GoogleLoginRedirect), "Users", null, Request.Scheme);
            var properties = new AuthenticationProperties { RedirectUri = redirectUri };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-login-redirect")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLoginRedirect()
        {

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value;
            var googleId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(googleId))
            {
                return BadRequest("Google authentication failed. User ID claim is missing.");
            }

            var user = await _userRepository.GetUserByGoogleIdAsync(googleId);
            if (user == null)
            {
                user = new User
                {
                    UserId = Guid.NewGuid().ToString(),
                    GoogleId = googleId,
                    Username = email?.Split('@')[0],
                    Email = email,
                    FullName = name,
                    ProfilePictureUrl = User.FindFirst("picture")?.Value, 
                    CreatedAt = DateTime.UtcNow
                };
                await _userRepository.AddUserAsync(user);
            }
            else
            {
             
                user.Email = email;
                user.FullName = name;
                user.ProfilePictureUrl = User.FindFirst("picture")?.Value;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
            }

            var identity = (ClaimsIdentity)User.Identity;
            var principal = (ClaimsPrincipal)User;


            if (!identity.HasClaim(c => c.Type == ClaimTypes.Role))
            {

                identity.AddClaim(new Claim(ClaimTypes.Role, "Member"));


                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }

            var jwt = _jwtGenerator.GenerateToken(user.UserId, user.Username);

            
            return Ok(new AuthResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Token = jwt
            });
        }
        
        [HttpGet("{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(UserProfile), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserProfile(string userId)
        {
            try
            {
                var query = new GetUserProfileQuery { UserId = userId };
                var userProfile = await _mediator.Send(query);
                if (userProfile == null)
                {
                    return NotFound();
                }
                return Ok(userProfile);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the user profile." });
            }
        }
        
        [HttpPut("{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(UserProfile), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserProfile(string userId, [FromBody] UserProfileUpdate request)
        {
            try
            {
                if (GetCurrentUserId() != userId)
                {
                    return Forbid();
                }
        
                var command = new UpdateUserProfileCommand
                {
                    UserId = userId,
                    FullName = request.FullName,
                    Bio = request.Bio,
                    ProfilePictureUrl = request.ProfilePictureUrl
                };
                var updatedProfile = await _mediator.Send(command);
                return Ok(updatedProfile);
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
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while updating the user profile." });
            }
        }
        
        
        [HttpPost("{userId}/follow")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FollowUser(string userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var command = new FollowUserCommand { FollowerId = currentUserId, FollowingId = userId };
                await _mediator.Send(command);
                return Ok(new { message = "User followed successfully." });
            }
            catch (ApplicationException ex)
            {
                if (ex.Message.Contains("not found")) return NotFound(new { message = ex.Message });
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while trying to follow the user." });
            }
        }
        
        [HttpDelete("{userId}/unfollow")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UnfollowUser(string userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var command = new UnfollowUserCommand { FollowerId = currentUserId, FollowingId = userId };
                await _mediator.Send(command);
                return Ok(new { message = "User unfollowed successfully." });
            }
            catch (ApplicationException ex)
            {
                if (ex.Message.Contains("not found")) return NotFound(new { message = ex.Message });
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while trying to unfollow the user." });
            }
        }
        
        [HttpGet("{userId}/followers")]
        [ProducesResponseType(typeof(IEnumerable<UserSummary>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserFollowers(string userId)
        {
            try
            {
                var query = new GetUserFollowersQuery { UserId = userId };
                var followers = await _mediator.Send(query);
                return Ok(followers);
            }
            catch (ApplicationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving followers." });
            }
        }
        
        [HttpGet("{userId}/following")]
        [ProducesResponseType(typeof(IEnumerable<UserSummary>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserFollowing(string userId)
        {
            try
            {
                var query = new GetUserFollowingQuery { UserId = userId };
                var following = await _mediator.Send(query);
                return Ok(following);
            }
            catch (ApplicationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving following users." });
            }
        }
    }
}
