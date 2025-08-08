using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using social_media9.Api.DTOs;
using social_media9.Api.Models;

namespace social_media9.Api.Services.Interfaces
{
    public interface ILikeService
    {
        Task<LikeResponse> LikePostAsync(string postId, string userId, string username);
        Task<bool> UnlikePostAsync(string postId, string userId);
        Task<PostLikesResponse> GetPostLikesAsync(string postId, string? currentUserId = null);
        Task<bool> IsPostLikedByUserAsync(string postId, string userId);
        Task<List<string>> GetLikedPostsByUserAsync(string userId);
        Task<Dictionary<string, bool>> GetPostsLikedStatusAsync(List<string> postIds, string userId);
    }
}