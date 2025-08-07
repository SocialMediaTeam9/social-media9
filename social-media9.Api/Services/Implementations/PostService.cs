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


        public PostService(
            IPostRepository postRepository,
            IAmazonSQS sqsClient,
            DynamoDbService dbService,
            IUserRepository userRepository,
            IConfiguration config, ILogger<PostService> logger, IHttpClientFactory httpClientFactory)
        {
            _postRepository = postRepository;
            _dbService = dbService;
            _sqsClient = sqsClient;
            _config = config;
            _userRepository = userRepository;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        // public async Task<Guid> CreatePostAsync(CreatePostRequest request, Guid userId)
        // {
        //     string? mediaUrl = null;

        //     // Upload media if present
        //     if (request.MediaFile != null)
        //     {
        //         mediaUrl = await _storageService.UploadFileAsync(request.MediaFile);
        //     }

        //     var post = new Post
        //     {
        //         // Id = Guid.NewGuid(),
        //         Content = request.Content,
        //         MediaUrl = mediaUrl,
        //         MediaType = request.MediaType ?? "none",
        //         UserId = userId,
        //         CreatedAt = DateTime.UtcNow
        //     };

        //     await _postRepository.AddAsync(post);
        //     return post.Id;
        // }

        // public async Task<IEnumerable<PostDTO>> GetPostsAsync()
        // {

        //     _dbService.GetPostByIdAsync

        //     var posts = await _postRepository.GetAllAsync();
        //     return posts.Select(post => new PostDTO
        //     {

        //         Content = post.Content,
        //         MediaUrl = post.MediaUrl,
        //         MediaType = post.MediaType,
        //         UserId = post.UserId,
        //         CreatedAt = post.CreatedAt
        //     });
        // }


        // // Get posts by user
        // public async Task<IEnumerable<PostDTO>> GetUserPostsAsync(Guid userId)
        // {
        //     // TODO: Implement get posts by user logic
        //     throw new NotImplementedException();
        // }

        // Like post
        // public async Task<bool> LikePostAsync(Guid postId, Guid userId)
        // {
        //     // Check if already liked (optional, for idempotency)
        //     return await _postRepository.LikeAsync(postId, userId);
        // }

        // // Get post likes
        // public async Task<IEnumerable<UserSummaryDTO>> GetPostLikesAsync(Guid postId)
        // {
        //     // TODO: Implement get post likes logic
        //     throw new NotImplementedException();
        // }

        // Add comment
        /* public async Task<Guid> AddCommentAsync(Guid postId, AddCommentRequest request, Guid userId)
         {
             // For now, username is not fetched; you may want to fetch it from user service/repo
             var comment = new Comment
             {
                 CommentId = Guid.NewGuid().ToString(),
                 PostId = postId.ToString(),
                 UserId = userId.ToString(),
                 Username = "", // TODO: fetch username from user service if needed
                 Text = request.Text,
                 CreatedAt = DateTime.UtcNow
             };
             await _postRepository.AddCommentAsync(postId, request, userId);
             return comment.CommentId;
         }

         // Get comments
         public async Task<IEnumerable<CommentDTO>> GetCommentsAsync(Guid postId)
         {
             // TODO: Implement get comments logic
             throw new NotImplementedException();
         }*/

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
            var authorActorUrl = $"https://fed.{domain}/users/{authorUsername}";
            var postUrl = $"https://fed.{domain}/users/{authorUsername}/posts/{postId}";


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
                return null;
            }

            _ = DeliverPostToFollowersAsync(activityDoc, author);

            return newPost;
        }

        private async Task DeliverPostToFollowersAsync(JsonDocument activityDoc, User author)
        {
            var followers = await _dbService.GetFollowersAsync(author.Username);
            if (!followers.Any()) return;

            _logger.LogInformation("Starting outbound delivery of post by {Username} to {FollowerCount} followers.", author.Username, followers.Count);


            var httpClient = _httpClientFactory.CreateClient("FederationClient");

            var domain = _config["DomainName"];
            var actorUrl = $"https://{domain}/users/{author.Username}";
            var deliveryService = new ActivityPubService(httpClient, actorUrl, author.PrivateKeyPem);

            var deliveryTasks = followers.Select(follower =>
            {
                var targetInbox = $"{follower.FollowerInfo.ActorUrl}/inbox";
                return deliveryService.DeliverActivityAsync(targetInbox, activityDoc);
            });

            await Task.WhenAll(deliveryTasks);

            _logger.LogInformation("Completed outbound delivery for post by {Username}", author.Username);
        }


        private string BuildCreateNoteActivityJson(string authorActorUrl, string postUrl, string content, List<string>? attachmentUrls)
        {

            var apAttachments = (attachmentUrls ?? new List<string>())
            .Select(url => new
            {
                type = "Document", // Or "Image", "Video" if you detect the MIME type
                mediaType = "image/jpeg",
                url = url
            }).ToList();

            var activity = new
            {
                type = "Create",
                actor = authorActorUrl,
                to = new[] { "https://www.w3.org/ns/activitystreams#Public" },
                cc = new[] { $"{authorActorUrl}/followers" },
                @object = new
                {
                    id = postUrl,
                    type = "Note",
                    published = DateTime.UtcNow.ToString("o"),
                    attributedTo = authorActorUrl,
                    content = content,
                    to = new[] { "https://www.w3.org/ns/activitystreams#Public" },
                    cc = new[] { $"{authorActorUrl}/followers" },
                    attachment = apAttachments
                }
            };

            // Serialize with an explicit context for the '@' symbol
            var jsonObject = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(activity));
            jsonObject["@context"] = "https://www.w3.org/ns/activitystreams";

            return JsonSerializer.Serialize(jsonObject);
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