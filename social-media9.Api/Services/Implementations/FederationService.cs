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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FederationService> _logger;

        public FederationService(IHttpClientFactory httpClient, IUserRepository userRepository, IHttpClientFactory httpClientFactory, ILogger<FederationService> logger)
        {
            _httpClient = httpClient.CreateClient();
            _userRepository = userRepository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        private string ExtractUsernameFromActorUrl(string url) => url.Split('/').Last();

        public async Task<List<PostResponse>> GetRemoteUserOutboxAsync(string actorUrl)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("FederationClient");
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var actorRequest = new HttpRequestMessage(HttpMethod.Get, actorUrl);
                actorRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/activity+json"));
                var actorHttpResponse = await httpClient.SendAsync(actorRequest);
                actorHttpResponse.EnsureSuccessStatusCode();

                var actorData = await actorHttpResponse.Content.ReadFromJsonAsync<ActorResponse>(jsonOptions);
                if (string.IsNullOrEmpty(actorData?.Outbox))
                {
                    _logger.LogWarning("Remote actor {ActorUrl} does not have an outbox property.", actorUrl);
                    return new List<PostResponse>();
                }

                var outboxCollection = await httpClient.GetFromJsonAsync<OrderedCollection>(actorData.Outbox, jsonOptions);
                if (string.IsNullOrEmpty(outboxCollection?.First)) return new List<PostResponse>();

                var outboxPage = await httpClient.GetFromJsonAsync<OrderedCollectionPage>(outboxCollection.First, jsonOptions);
                if (outboxPage?.OrderedItems == null) return new List<PostResponse>();

                var posts = new List<PostResponse>();
                foreach (var item in outboxPage.OrderedItems)
                {
                    var activity = JsonDocument.Parse(JsonSerializer.Serialize(item)).RootElement;
                    if (activity.TryGetProperty("type", out var type) && type.GetString() == "Create" &&
                        activity.TryGetProperty("object", out var postObject))
                    {
                        posts.Add(ParseNoteObjectToPostResponse(postObject));
                    }
                }
                return posts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch remote outbox for actor {ActorUrl}", actorUrl);
                throw new ApplicationException("Could not retrieve posts from the remote server.");
            }
        }

        private PostResponse ParseNoteObjectToPostResponse(JsonElement postObject)
        {
            string postId = postObject.TryGetProperty("id", out var id) ? id.GetString() ?? "" : "";
            string authorUsername = postObject.TryGetProperty("attributedTo", out var author) ? author.GetString()?.Split('/').Last() ?? "" : "";
            string content = postObject.TryGetProperty("content", out var c) ? c.GetString() ?? "" : "";
            string createdAtStr = postObject.TryGetProperty("published", out var p) ? p.GetString() ?? "" : "";

            var attachments = new List<string>();
            if (postObject.TryGetProperty("attachment", out var atts) && atts.ValueKind == JsonValueKind.Array)
            {
                attachments.AddRange(atts.EnumerateArray()
                    .Select(a => a.TryGetProperty("url", out var url) ? url.GetString() : null)
                    .Where(u => u != null)!);
            }

            return new PostResponse(
                PostId: postId,
                AuthorUsername: authorUsername,
                Content: content,
                CreatedAt: DateTime.TryParse(createdAtStr, out var dt) ? dt : DateTime.UtcNow,
                CommentCount: 0, // We don't fetch remote comment counts in this simple lookup
                Attachments: attachments
            );
        }

        private async Task<string?> ResolveInboxUrlAsync(string actorUrl)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("FederationClient");
                var res = await client.GetAsync(actorUrl).ConfigureAwait(false);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch actor document {ActorUrl}: {Status}", actorUrl, res.StatusCode);
                    return null;
                }

                var body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                using var doc = JsonDocument.Parse(body);

                if (doc.RootElement.TryGetProperty("inbox", out var inboxProp) && inboxProp.ValueKind == JsonValueKind.String)
                {
                    return inboxProp.GetString();
                }

                if (doc.RootElement.TryGetProperty("endpoints", out var endpoints) &&
                    endpoints.ValueKind == JsonValueKind.Object &&
                    endpoints.TryGetProperty("inbox", out var inbox2) &&
                    inbox2.ValueKind == JsonValueKind.String)
                {
                    return inbox2.GetString();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not resolve inbox for actor {Actor}", actorUrl);
                return null;
            }
        }

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

        Task<string?> IFederationService.ResolveInboxUrlAsync(string actorUrl)
        {
            return ResolveInboxUrlAsync(actorUrl);
        }
    }
}