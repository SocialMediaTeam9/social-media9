using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using social_media9.Api.Models;
using social_media9.Api.Dtos;

namespace social_media9.Api.Repositories.Interfaces
{
    public interface IPostRepository
    {
        Task AddAsync(Post post);                     // Create post
        Task<IEnumerable<Post>> GetAllAsync();        // Get all posts (global feed)
        Task<Post?> GetByIdAsync(Guid postId);        // Get single post by ID
        Task<IEnumerable<Post>> GetByUserAsync(Guid userId); // Get posts by a specific user

        Task<bool> LikeAsync(Guid postId, Guid userId);   // Like post
        Task<bool> UnlikeAsync(Guid postId, Guid userId); // Unlike post

        // These are optional for future expansion
        // Task<bool> UpdateAsync(Guid postId, PostUpdateRequest request, Guid userId);
        // Task<bool> DeleteAsync(Guid postId, Guid userId);
        // Task<IEnumerable<UserSummaryDTO>> GetLikesAsync(Guid postId);
        // Task<Guid> AddCommentAsync(Guid postId, AddCommentRequest request, Guid userId);
        // Task<IEnumerable<CommentDTO>> GetCommentsAsync(Guid postId);
    }
}