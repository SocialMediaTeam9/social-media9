using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using social_media9.Api.Models;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Services.Interfaces;
using System;
using System.Linq;
using System.Net.Http.Headers;

namespace social_media9.Api.Services.Implementations
{
    public class FederationService : IFederationService
    {
        private readonly HttpClient _httpClient;
        private readonly IUserRepository _userRepository;

        public FederationService(IHttpClientFactory httpClientFactory, IUserRepository userRepository)
        {
            _httpClient = httpClientFactory.CreateClient();
            _userRepository = userRepository;
        }
        
        private string ExtractUsernameFromActorUrl(string url) => url.Split('/').Last();

        public async Task<User?> DiscoverAndCacheUserAsync(string userHandle)
        {
            var parts = userHandle.Split('@');
            if (parts.Length != 2) return null; // Invalid handle

            var username = parts[0];
            var domain = parts[1];

            try
            {
                // 1. Perform WebFinger lookup to get the Actor URL
                var webFingerUrl = $"https://{domain}/.well-known/webfinger?resource=acct:{userHandle}";
                var webFingerResponse = await _httpClient.GetAsync(webFingerUrl);
                webFingerResponse.EnsureSuccessStatusCode();

                var webFingerData = await JsonSerializer.DeserializeAsync<WebFingerResponse>(
                    await webFingerResponse.Content.ReadAsStreamAsync());

                var actorLink = webFingerData?.Links.FirstOrDefault(l => l.Rel == "self" && l.Type == "application/activity+json");
                if (actorLink?.Href == null) return null;

                var username_ = ExtractUsernameFromActorUrl(actorLink.Href);

                var existingUser = await _userRepository.GetUserByUsernameAsync(username_);

                if (existingUser != null)
                {
                    return existingUser!;
                }

                // 3. Fetch the Actor profile
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/activity+json"));

                var actorResponse = await _httpClient.GetAsync(actorLink.Href);
                actorResponse.EnsureSuccessStatusCode();

                var actorData = await JsonSerializer.DeserializeAsync<ActorResponse>(
                    await actorResponse.Content.ReadAsStreamAsync());
                if (actorData == null) return null;

                var newUser = new User
                {
                    PK = $"USER#{actorData.PreferredUsername}@{domain}",
                    SK = "METADATA",
                    UserId = Guid.NewGuid().ToString(),
                    Username = actorData.PreferredUsername ?? "",
                    FullName = actorData.Name ?? actorData.PreferredUsername ?? "",
                    ProfilePictureUrl = "",
                    PublicKeyPem = actorData.PublicKey.PublicKeyPem,
                    PrivateKeyPem = "",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsRemote = true,
                    ActorUrl = actorData.Id,
                    InboxUrl = actorData.Inbox,
                    FollowersUrl = actorData.Followers
                };

                await _userRepository.AddUserAsync(newUser);

                return newUser;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FederationService] Failed to discover user {userHandle}: {ex.Message}");
                return null;
            }
        }
    }
}