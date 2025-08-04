namespace social_media9.Api.Services.Interfaces
{
    public interface IGoogleAuthService
    {
        Task<GoogleUserInfo?> GetUserInfoAsync(string accessToken);
        Task<GoogleTokenResponse?> ExchangeCodeForTokenAsync(string code, string redirectUri);
    }

    public class GoogleTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string IdToken { get; set; } = string.Empty; // Contains user info
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = string.Empty;
    }

    public class GoogleUserInfo
    {
        public string Id { get; set; } = string.Empty; // Google's unique user ID
        public string Email { get; set; } = string.Empty;
        public bool VerifiedEmail { get; set; }
        public string Name { get; set; } = string.Empty;
        public string GivenName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty; // Profile picture URL
        public string Locale { get; set; } = string.Empty;
    }
}
