using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using social_media9.Api.Queries;
using System.Threading.Tasks;

namespace Peerspace.Api.Controllers
{
    [ApiController]
    [Route("/api/follow")] // The base route for this controller
    [Authorize] // All actions related to following require a logged-in user
    public class FollowController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FollowController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Checks if a given user is following another user.
        /// </summary>
        /// <param name="localUsername">The username of the potential follower.</param>
        /// <param name="targetUsername">The username of the user who is potentially being followed.</param>
        /// <returns>A boolean indicating if the follow relationship exists.</returns>
        [HttpGet("is-following")]
        public async Task<IActionResult> IsFollowing(
            [FromQuery] string localUsername, 
            [FromQuery] string targetUsername)
        {
            if (string.IsNullOrWhiteSpace(localUsername) || string.IsNullOrWhiteSpace(targetUsername))
            {
                return BadRequest("Both 'localUsername' and 'targetUsername' query parameters are required.");
            }

            try
            {
                var query = new IsFollowingQuery(localUsername, targetUsername);
                bool isFollowing = await _mediator.Send(query);

                return Ok(new { isFollowing });
            }
            catch (Exception ex)
            {
                // In a real app, you would log the exception
                return StatusCode(500, new { message = "An error occurred while checking the follow status." });
            }
        }
    }
}