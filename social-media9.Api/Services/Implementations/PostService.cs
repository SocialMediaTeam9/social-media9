using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using social_media9.Api.Models;
using social_media9.Api.Dtos;

using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Services;
using social_media9.Api.Services.Interfaces;
using social_media9.Api.Services.DynamoDB;
using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using Amazon.DynamoDBv2.DocumentModel;

namespace social_media9.Api.Services.Implementations
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;
        private readonly DynamoDbService _dbService;
        private readonly IAmazonSQS _sqsClient;
        private readonly IConfiguration _config;

        private readonly ILogger<PostService> _logger;

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IFollowRepository _followRepository;
        private readonly Amazon.DynamoDBv2.DataModel.IDynamoDBContext _dbContext;

        private readonly IFederationService _federationService;

        public PostService(
            IPostRepository postRepository,
            IAmazonSQS sqsClient,
            DynamoDbService dbService,
            IUserRepository userRepository,
            IConfiguration config, ILogger<PostService> logger, IHttpClientFactory httpClientFactory,
            IFollowRepository followRepository,
            Amazon.DynamoDBv2.DataModel.IDynamoDBContext dbContext,
            IFederationService federationService)
        {
            _postRepository = postRepository;
            _dbService = dbService;
            _sqsClient = sqsClient;
            _config = config;
            _userRepository = userRepository;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _followRepository = followRepository;
            _dbContext = dbContext;
            _federationService = federationService;
        }

        public async Task<IEnumerable<Post>> GetUserPostsAsync(String username)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<LikeEntity>> GetLikesAsync(string postId, int limit = 20)
        {
            return await _postRepository.GetLikesAsync(postId);
        }

        public async Task<bool> LikePostAsync(string postId, string likerUsername)
        {
            var likerSummary = await _userRepository.GetUserSummaryAsync(likerUsername);
            if (likerSummary == null)
            {
                return false;
            }

            return await _dbService.LikePostAsync(postId, likerSummary);
        }

        public async Task<Post?> GetPostByIdAsync(string postId)
        {
            return await _dbService.GetPostByIdAsync(postId);
        }



        public async Task IngestRemotePostAsync(JsonElement createActivity)
        {
            try
            {
                // 1. Extract the post object from the "Create" activity
                if (!createActivity.TryGetProperty("object", out var postObject)) return;
                if (postObject.TryGetProperty("type", out var type) && type.GetString() != "Note") return;

                var authorActorUrl = postObject.GetProperty("attributedTo").GetString();
                if (authorActorUrl == null) return;

                // 2. Discover and cache the author if we've never seen them before
                var author = await _userRepository.GetUserByActorUrl(authorActorUrl);
                if (author == null)
                {
                    // This is a simplified discovery, a real app might need a dedicated FederationService call
                    var handle = $"{authorActorUrl.Split('/')[4]}@{new Uri(authorActorUrl).Host}";
                    author = await _federationService.DiscoverAndCacheUserAsync(handle);
                }
                if (author == null) return; // Could not find the author, skip post

                // 3. Create a Post object from the incoming data and save it
                // You will need to add a static helper method to your Post model for this.
                var newPost = Post.FromActivityPub(postObject, author.Username);
                await _postRepository.AddAsync(newPost); // Save to your Posts table

                // 4. Fan the post out to your public timeline
                await FanoutPostToPublicTimelineAsync(newPost);
                _logger.LogInformation("Successfully ingested and fanned out a remote post from {Author}", author.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ingest remote post.");
            }
        }

        public async Task<Post?> CreateAndFederatePostAsync(string authorUsername, string content, List<string>? attachmentUrls)
        {
            var author = await _dbService.GetUserProfileByUsernameAsync(authorUsername);

            if (author == null || string.IsNullOrEmpty(author.PrivateKeyPem))
            {
                _logger.LogError("Cannot federate post for user {Username} because they do not exist or have no private key.", authorUsername);
                return null;
            }

            var postId = Ulid.NewUlid().ToString();
            var domain = _config["DomainName"];
            var authorActorUrl = $"https://{domain}/users/{authorUsername}";
            var postUrl = $"https://{domain}/users/{authorUsername}/posts/{postId}";

            // Build ActivityPub Create Note JSON
            string activityJson = BuildCreateNoteActivityJson(authorActorUrl, postUrl, content, attachmentUrls);

            var newPost = new Post
            {
                PK = $"POST#{postId}",
                SK = $"POST#{postId}",
                GSI1PK = $"USER#{authorUsername}",
                GSI1SK = $"POST#{postId}",
                AuthorUsername = authorUsername,
                Content = content,
                ActivityJson = activityJson,
                CreatedAt = DateTime.UtcNow,
                CommentCount = 0,
                Attachments = attachmentUrls ?? new List<string>()
            };

            var activityDoc = JsonDocument.Parse(activityJson);

            bool dbSuccess = await _dbService.CreatePostAsync(newPost);
            if (!dbSuccess)
            {
                _logger.LogError("Failed to save post {PostId} for user {Username}", postId, authorUsername);
                return null;
            }

            // Fan-out locally (immediate UI updates)
            _ = FanoutPostToLocalFollowersAsync(newPost);
            _ = FanoutPostToPublicTimelineAsync(newPost);

            // Deliver instantly to remote followers
            _ = DeliverPostToFollowersAsync(activityDoc, author);

            return newPost;
        }


        private async Task DeliverPostToFollowersAsync(JsonDocument activityDoc, User author)
        {
            string? paginationToken = null;
            int pageNumber = 1;
            int totalDelivered = 0;
            const int pageSize = 50;

            var httpClient = _httpClientFactory.CreateClient("FederationClient");
            var domain = _config["DomainName"];
            var actorUrl = $"https://{domain}/users/{author.Username}";
            var deliveryService = new ActivityPubService(httpClient, actorUrl, author.PrivateKeyPem, _config);

            do
            {
                _logger.LogInformation("Fetching page {PageNumber} of followers for {Username}.", pageNumber, author.Username);

                var (followers, nextToken) = await _dbService.GetFollowersAsync(author.Username, pageSize, paginationToken);

                if (!followers.Any())
                    break;

                foreach (var follower in followers)
                {
                    var inboxUrl = await ResolveInboxUrlAsync(follower.FollowerInfo.ActorUrl);
                    if (string.IsNullOrEmpty(inboxUrl))
                    {
                        _logger.LogWarning("Could not resolve inbox for {ActorUrl}", follower.FollowerInfo.ActorUrl);
                        continue;
                    }

                    await deliveryService.DeliverActivityAsync(inboxUrl, activityDoc);
                }

                totalDelivered += followers.Count;
                _logger.LogInformation("Delivered batch {PageNumber} for {Username}. Total so far: {Total}",
                    pageNumber, author.Username, totalDelivered);

                paginationToken = nextToken;
                pageNumber++;

                if (paginationToken != null)
                    await Task.Delay(1000);

            } while (!string.IsNullOrEmpty(paginationToken));

            _logger.LogInformation("Completed outbound delivery for post by {Username}. Total followers reached: {TotalDelivered}",
                author.Username, totalDelivered);
        }
        private async Task<string?> ResolveInboxUrlAsync(string actorUrl)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("FederationClient");
                var res = await client.GetAsync(actorUrl);
                if (!res.IsSuccessStatusCode)
                    return null;

                var body = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(body);

                if (doc.RootElement.TryGetProperty("inbox", out var inboxProp) &&
                    inboxProp.ValueKind == JsonValueKind.String)
                {
                    return inboxProp.GetString();
                }

                if (doc.RootElement.TryGetProperty("endpoints", out var endpoints) &&
                    endpoints.ValueKind == JsonValueKind.Object &&
                    endpoints.TryGetProperty("sharedInbox", out var sharedInbox) &&
                    sharedInbox.ValueKind == JsonValueKind.String)
                {
                    return sharedInbox.GetString();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not resolve inbox for actor {Actor}", actorUrl);
                return null;
            }
        }

        private string GuessMediaType(string url)
        {
            if (url.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) return "image/png";
            if (url.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)) return "image/gif";
            if (url.EndsWith(".webp", StringComparison.OrdinalIgnoreCase)) return "image/webp";
            if (url.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)) return "video/mp4";
            return "image/jpeg";
        }
        private async Task FanoutPostToLocalFollowersAsync(Post post)
        {
            try
            {
                var followers = await _followRepository.GetFollowersAsync(post.AuthorUsername);
                var timelineBatch = _dbContext.CreateBatchWrite<TimelineItemEntity>();
                bool itemsAdded = false;

                string postId = post.SK.Replace("POST#", "");
                var appDomain = _config["DomainName"];

                if (followers.Any())
                {
                    foreach (var followRelationship in followers)
                    {
                        var followerDomain = new Uri(followRelationship.FollowerInfo.ActorUrl).Host;
                        if (followerDomain.Equals(appDomain, StringComparison.OrdinalIgnoreCase))
                        {
                            var followerUsername = followRelationship.FollowerInfo.ActorUrl.Split('/').Last();
                            timelineBatch.AddPutItem(new TimelineItemEntity
                            {
                                PK = $"TIMELINE#{followerUsername}",
                                SK = $"NOTE#{postId}",
                                AuthorUsername = post.AuthorUsername,
                                PostContent = post.Content,
                                AttachmentUrls = post.Attachments,
                                CreatedAt = post.CreatedAt
                            });
                            itemsAdded = true;
                        }
                    }
                }

                timelineBatch.AddPutItem(new TimelineItemEntity
                {
                    PK = $"TIMELINE#{post.AuthorUsername}",
                    SK = $"NOTE#{postId}",
                    AuthorUsername = post.AuthorUsername,
                    PostContent = post.Content,
                    AttachmentUrls = post.Attachments,
                    CreatedAt = post.CreatedAt
                });
                itemsAdded = true;

                if (!itemsAdded) return;

                _logger.LogInformation("Fanning out post {PostId} to local timelines.", postId);
                await timelineBatch.ExecuteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fan-out post {PostId} to local timelines.", post.SK);
            }
        }

        private async Task FanoutPostToPublicTimelineAsync(Post post)
        {
            try
            {
                var invertedTicks = DateTime.MaxValue.Ticks - post.CreatedAt.Ticks;

                var timelineItem = new TimelineItemEntity
                {
                    PK = "TIMELINE#PUBLIC",
                    // The SK format includes a timestamp to ensure chronological order
                    SK = $"NOTE#{post.CreatedAt:o}#{post.SK.Replace("POST#", "")}",
                    AuthorUsername = post.AuthorUsername,
                    PostContent = post.Content,
                    AttachmentUrls = post.Attachments,
                    CreatedAt = post.CreatedAt
                };

                // Note: This requires injecting IDynamoDBContext into PostService
                await _dbContext.SaveAsync(timelineItem);
                _logger.LogInformation("Added post {PostId} to the public timeline.", post.SK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add post {PostId} to the public timeline.", post.SK);
            }
        }


        private string BuildCreateNoteActivityJson(string authorActorUrl, string postUrl, string content, List<string>? attachmentUrls)
        {
            var attachments = (attachmentUrls ?? new List<string>()).Select(url => new Dictionary<string, object>
            {
                ["type"] = "Document",
                ["mediaType"] = GuessMediaType(url),
                ["url"] = url
            }).ToList();

            var note = new Dictionary<string, object>
            {
                ["id"] = postUrl,
                ["type"] = "Note",
                ["published"] = DateTime.UtcNow.ToString("o"),
                ["attributedTo"] = authorActorUrl,
                ["content"] = content,
                ["to"] = new[] { "https://www.w3.org/ns/activitystreams#Public" },
                ["cc"] = new[] { $"{authorActorUrl}/followers" },
            };

            if (attachments.Any())
                note["attachment"] = attachments;

            var activity = new Dictionary<string, object>
            {
                ["@context"] = "https://www.w3.org/ns/activitystreams",
                ["id"] = $"{postUrl}#create", // unique id for the activity itself
                ["type"] = "Create",
                ["actor"] = authorActorUrl,
                ["to"] = new[] { "https://www.w3.org/ns/activitystreams#Public" },
                ["cc"] = new[] { $"{authorActorUrl}/followers" },
                ["object"] = note
            };

            // Use JsonSerializer with explicit options if you want stable ordering
            return JsonSerializer.Serialize(activity);
        }

        public async Task<Comment?> AddCommentAsync(string postId, string authorUsername, string content)
        {
            var commentId = Ulid.NewUlid().ToString();
            var newComment = new Comment
            {
                PK = $"POST#{postId}",
                SK = $"COMMENT#{commentId}",
                GSI1PK = $"USER#{authorUsername}",
                GSI1SK = $"COMMENT#{commentId}",
                Username = authorUsername,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            bool success = await _dbService.CreateCommentAsync(newComment);
            return success ? newComment : null;
        }

        public async Task<IEnumerable<Comment>> GetCommentsForPostAsync(string postId)
        {
            return await _postRepository.GetCommentsAsync(postId);
        }

    }
}