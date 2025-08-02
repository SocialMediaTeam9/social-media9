using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using social_media9.Api.Models;
using social_media9.Api.Dtos;

namespace social_media9.Api.Repositories.Interfaces
{
    public interface IPostRepository
    {
        Task AddAsync(Post post);
        Task<IEnumerable<Post>> GetAllAsync();
        Task<Post> GetByIdAsync(Guid postId);
        // Task<bool> UpdateAsync(Guid postId, PostUpdateRequest request, Guid userId);
        // Task<bool> DeleteAsync(Guid postId, Guid userId);
        Task<IEnumerable<Post>> GetByUserAsync(Guid userId);
        Task<bool> LikeAsync(Guid postId, Guid userId);
        Task<bool> UnlikeAsync(Guid postId, Guid userId);
        // Task<IEnumerable<UserSummaryDTO>> GetLikesAsync(Guid postId);
        Task<Guid> AddCommentAsync(Guid postId, AddCommentRequest request, Guid userId);
        Task<IEnumerable<CommentDTO>> GetCommentsAsync(Guid postId);
    }
}