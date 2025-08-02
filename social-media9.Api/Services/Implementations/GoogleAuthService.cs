using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace social_media9.Api.Services.Implementations
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public GoogleAuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _clientId = _configuration["GoogleAuthSettings:ClientId"] ?? throw new InvalidOperationException("Google ClientId not configured.");
            _clientSecret = _configuration["GoogleAuthSettings:ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret not configured.");
        }

        public async Task<GoogleTokenResponse?> ExchangeCodeForTokenAsync(string code, string redirectUri)
        {
            var tokenEndpoint = "https://oauth2.googleapis.com/token";
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" }
            });

            var response = await _httpClient.PostAsync(tokenEndpoint, content);
            response.EnsureSuccessStatusCode(); // Throws if not 2xx

            return await response.Content.ReadFromJsonAsync<GoogleTokenResponse>();
        }

        public async Task<GoogleUserInfo?> GetUserInfoAsync(string accessToken)
        {
            var userInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync(userInfoEndpoint);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<GoogleUserInfo>();
        }
    }
}