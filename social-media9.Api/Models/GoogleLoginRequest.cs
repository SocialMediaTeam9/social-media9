
namespace social_media9.Api.Models
{
    public class GoogleLoginRequest
    {
        public string Code { get; set; } = string.Empty; // The authorization code from Google
        public string RedirectUri { get; set; } = string.Empty; // The redirect URI used in the frontend
    }
}
