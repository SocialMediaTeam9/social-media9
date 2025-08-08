using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using social_media9.Api.Models;
using social_media9.Api.DTOs;
using social_media9.Api.Data;
using social_media9.Api.Services.Interfaces;

namespace social_media9.Api.Services
{
    public class LikeService : ILikeService
    {
        private readonly IDynamoDBContext _context;
        private readonly IAmazonDynamoDB _client;

        public LikeService(DynamoDbClientFactory factory)
        {
            _context = factory.GetContext();
            _client = factory.GetClient();
        }

        public async Task<LikeResponse> LikePostAsync(string postId, string userId, string username)
        {
            // Check if already liked
            var existingLike = await _context.LoadAsync<Like>(postId, userId);
            if (existingLike != null)
            {
                return new LikeResponse(existingLike.LikeId, existingLike.PostId, existingLike.UserId, existingLike.CreatedAt);
            }

            var like = new Like
            {
                PostId = postId,
                UserId = userId,
                Username = username,
                CreatedAt = DateTime.UtcNow,
                LikeId = Guid.NewGuid().ToString()
            };

            await _context.SaveAsync(like);
            return new LikeResponse(like.LikeId, like.PostId, like.UserId, like.CreatedAt);
        }

        public async Task<bool> UnlikePostAsync(string postId, string userId)
        {
            var existingLike = await _context.LoadAsync<Like>(postId, userId);
            if (existingLike == null)
            {
                return false;
            }

            await _context.DeleteAsync<Like>(postId, userId);
            return true;
        }

        public async Task<PostLikesResponse> GetPostLikesAsync(string postId, string? currentUserId = null)
        {
            var queryConfig = new QueryOperationConfig
            {
                KeyExpression = new Expression
                {
                    ExpressionStatement = "PostId = :postId",
                    ExpressionAttributeValues = { [":postId"] = postId }
                }
            };

            var search = _context.FromQueryAsync<Like>(queryConfig);
            var likes = await search.GetRemainingAsync();

            var likeResponses = likes.Select(l => new LikeResponse(l.LikeId, l.PostId, l.UserId, l.CreatedAt)).ToList();
            var isLikedByUser = currentUserId != null && likes.Any(l => l.UserId == currentUserId);

            return new PostLikesResponse(postId, likes.Count, isLikedByUser, likeResponses);
        }

        public async Task<bool> IsPostLikedByUserAsync(string postId, string userId)
        {
            var like = await _context.LoadAsync<Like>(postId, userId);
            return like != null;
        }

        public async Task<List<string>> GetLikedPostsByUserAsync(string userId)
        {
            var scanConfig = new ScanOperationConfig
            {
                Filter = new ScanFilter(),
                Select = SelectValues.SpecificAttributes,
                AttributesToGet = new List<string> { "PostId" }
            };
            scanConfig.Filter.AddCondition("UserId", ScanOperator.Equal, userId);

            var search = _context.FromScanAsync<Like>(scanConfig);
            var likes = await search.GetRemainingAsync();
            return likes.Select(l => l.PostId).ToList();
        }

        public async Task<Dictionary<string, bool>> GetPostsLikedStatusAsync(List<string> postIds, string userId)
        {
            var result = new Dictionary<string, bool>();
            
            foreach (var postId in postIds)
            {
                var isLiked = await IsPostLikedByUserAsync(postId, userId);
                result[postId] = isLiked;
            }
            
            return result;
        }
    }
}