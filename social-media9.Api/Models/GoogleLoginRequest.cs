
namespace social_media9.Api.Models
{
    public class GoogleLoginRequest
    {
        public string Code { get; set; } = string.Empty; 
        public string RedirectUri { get; set; } = string.Empty; 
    }
}
