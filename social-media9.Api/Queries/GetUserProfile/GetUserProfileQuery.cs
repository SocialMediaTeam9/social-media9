using MediatR;
using social_media9.Api.Models;

namespace social_media9.Api
{
    public class GetUserProfileQuery : IRequest<UserProfile?>
    {
        public string UserId { get; set; } = string.Empty; // Changed to string
    }
}