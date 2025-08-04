using MediatR;
using social_media9.Api.Models;

namespace social_media9.Api.Commands
{
    public class FollowUserCommand : IRequest<Unit>
    {
        public string FollowerId { get; set; } = string.Empty; // Changed to string
        public string FollowingId { get; set; } = string.Empty; // Changed to string
    }
}