using MediatR;
using social_media9.Api.Models;

namespace social_media9.Api
{
    public class UnfollowUserCommand : IRequest<Unit>
    {
        public string FollowerId { get; set; } = string.Empty; // Changed to string
        public string FollowingId { get; set; } = string.Empty; // Changed to string
    }
}
