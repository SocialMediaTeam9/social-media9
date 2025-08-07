using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using social_media9.Api.Models;
using social_media9.Api.Dtos;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Services.Interfaces;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using social_media9.Api.Data; // For DynamoDbClientFactory

namespace social_media9.Api.Services.Implementations
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IStorageService _storageService;
        private readonly IAmazonDynamoDB _dynamoDb;

        public PostService(IPostRepository postRepository, IStorageService storageService, DynamoDbClientFactory dynamoDbClientFactory)
        {
            _postRepository = postRepository;
            _storageService = storageService;
            _dynamoDb = dynamoDbClientFactory.GetClient();
        }

        public async Task<Guid> CreatePostAsync(CreatePostRequest request, Guid userId, IFormFile? file)
        {
            string? mediaUrl = null;

            // Upload media if present
            if (file != null)
            {
                mediaUrl = await _storageService.UploadFileAsync(file);
            }

            var post = new Post
            {
                PostId = Guid.NewGuid(),
                Content = request.Content,
                MediaUrl = mediaUrl,
                MediaType = request.MediaType ?? "none",
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _postRepository.AddAsync(post);
            return post.PostId;
        }

        public async Task<IEnumerable<PostDTO>> GetPostsAsync()
        {
            var posts = await _postRepository.GetAllAsync();
            return posts.Select(post => new PostDTO
            {
                PostId = post.PostId,
                Content = post.Content,
                MediaUrl = post.MediaUrl,
                MediaType = post.MediaType,
                UserId = post.UserId,
                CreatedAt = post.CreatedAt
            });
        }

        public async Task<PostDTO?> GetPostAsync(Guid postId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
                return null;

            return new PostDTO
            {
                PostId = post.PostId,
                UserId = post.UserId,
                Content = post.Content,
                MediaType = post.MediaType,
                MediaUrl = post.MediaUrl,
                CreatedAt = post.CreatedAt
            };
        }

        public async Task<bool> LikePostAsync(Guid postId, Guid userId)
        {
            // Optional: check if already liked for idempotency
            return await _postRepository.LikeAsync(postId, userId);
        }

        public async Task<IEnumerable<PostDTO>> GetUserPostsAsync(Guid userId)
        {
            var request = new QueryRequest
            {
                TableName = "Posts",
                IndexName = "UserId-CreatedAt-index",
                KeyConditionExpression = "UserId = :uid",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":uid", new AttributeValue { S = userId.ToString() } }
                },
                ScanIndexForward = false // descending by CreatedAt
            };

            var response = await _dynamoDb.QueryAsync(request);

            var posts = response.Items.Select(item => new PostDTO
            {
                PostId = Guid.Parse(item["PostId"].S),
                UserId = Guid.Parse(item["UserId"].S),
                Content = item.TryGetValue("Content", out var contentAttr) ? contentAttr.S : string.Empty,
                MediaType = item.TryGetValue("MediaType", out var mediaTypeAttr) ? mediaTypeAttr.S : "none",
                MediaUrl = item.TryGetValue("MediaUrl", out var mediaUrlAttr) ? mediaUrlAttr.S : null,
                CreatedAt = DateTime.Parse(item["CreatedAt"].S)
            });

            return posts;
        }

        /* Uncomment and implement when ready

        public async Task<Guid> AddCommentAsync(Guid postId, AddCommentRequest request, Guid userId)
        {
            var comment = new Comment
            {
                CommentId = Guid.NewGuid().ToString(),
                PostId = postId.ToString(),
                UserId = userId.ToString(),
                Username = "", // TODO: fetch username if needed
                Text = request.Text,
                CreatedAt = DateTime.UtcNow
            };
            await _postRepository.AddCommentAsync(postId, request, userId);
            return Guid.Parse(comment.CommentId);
        }

        public async Task<IEnumerable<CommentDTO>> GetCommentsAsync(Guid postId)
        {
            throw new NotImplementedException();
        }
        */
    }
}