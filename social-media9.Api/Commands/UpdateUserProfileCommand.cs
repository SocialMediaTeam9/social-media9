using MediatR;
using social_media9.Api.Models;

namespace social_media9.Api
{
    public class UpdateUserProfileCommand : IRequest<UserProfile>
    {
        public string UserId { get; set; } = string.Empty; // Changed to string
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}