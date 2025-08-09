using Amazon.SQS;
using Microsoft.Extensions.Logging;
using social_media9.Api.DTOs;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Services.DynamoDB;
using social_media9.Api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace social_media9.Api.Services
{
    public class LikeService : ILikeService
    {
        private readonly DynamoDbService _dbService;
        private readonly IUserRepository _userRepository;
        private readonly IFollowRepository _followRepository;
        private readonly IFederationService _federationService;
        private readonly ILogger<LikeService> _logger;

        public LikeService(
            DynamoDbService dbService,
            IUserRepository userRepository,
            IFollowRepository followRepository,
            IFederationService federationService,
            ILogger<LikeService> logger)
        {
            _dbService = dbService;
            _userRepository = userRepository;
            _followRepository = followRepository;
            _federationService = federationService;
            _logger = logger;
        }

        public async Task<LikeResponse> LikePostAsync(string postId, string userId, string username)
        {
            var likerSummary = await _userRepository.GetUserSummaryAsync(username);
            if (likerSummary == null)
            {
                _logger.LogWarning("User summary not found for username: {Username}", username);
                return new LikeResponse(string.Empty, string.Empty, string.Empty, DateTime.MinValue);
            }

            var success = await _dbService.LikePostAsync(postId, likerSummary);
            if (!success)
            {
                _logger.LogWarning("Failed to like post {PostId} by user {UserId}", postId, userId);
                return new LikeResponse(string.Empty, string.Empty, string.Empty, DateTime.MinValue);
            }

            return new LikeResponse(Guid.NewGuid().ToString(), postId, userId, DateTime.UtcNow);
        }

        public async Task<bool> UnlikePostAsync(string postId, string userId)
        {
            var success = await _dbService.UnlikePostAsync(postId, userId);
            if (!success)
            {
                _logger.LogWarning("Failed to unlike post {PostId} by user {UserId}", postId, userId);
                return false;
            }

            return true;
        }

        public async Task<PostLikesResponse> GetPostLikesAsync(string postId, string? currentUserId = null)
        {
            var likes = await _dbService.GetPostLikesAsync(postId);
            var likeResponses = likes.Select(l =>
                new LikeResponse(l.LikeId, l.PostId, l.UserId, l.CreatedAt)).ToList();

            bool isLikedByUser = currentUserId != null && likes.Any(l => l.UserId == currentUserId);

            return new PostLikesResponse(postId, likes.Count, isLikedByUser, likeResponses);
        }

        public async Task<bool> IsPostLikedByUserAsync(string postId, string userId)
        {
            return await _dbService.IsPostLikedByUserAsync(postId, userId);
        }

        public async Task<List<string>> GetLikedPostsByUserAsync(string userId)
        {
            return await _dbService.GetLikedPostsByUserAsync(userId);
        }

        public async Task<Dictionary<string, bool>> GetPostsLikedStatusAsync(List<string> postIds, string userId)
        {
            var result = new Dictionary<string, bool>();

            foreach (var postId in postIds)
            {
                result[postId] = await _dbService.IsPostLikedByUserAsync(postId, userId);
            }

            return result;
        }
    }
}