using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using social_media9.Api.Models;
using social_media9.Api.Dtos;

namespace social_media9.Api.Repositories.Interfaces
{
    public interface IPostRepository
    {
        Task<IEnumerable<Post>> GetPostsByUsernameAsync(string username, int limit = 20);

        Task<IEnumerable<Post>> GetAllAsync();

        Task<Post> GetByIdAsync(string postId);

        Task<IEnumerable<LikeEntity>> GetLikesAsync(string postId, int limit = 20);

        Task<bool> LikeAsync(Guid postId, Guid userId);

        Task<bool> UnlikeAsync(Guid postId, Guid userId);

        Task<IEnumerable<Comment>> GetCommentsAsync(string postId, int limit = 50);

        // Task<IEnumerable<UserSummaryDTO>> GetLikesAsync(Guid postId);
        //Task<Guid> AddCommentAsync(Guid postId, AddCommentRequest request, Guid userId);
        //Task<IEnumerable<CommentDTO>> GetCommentsAsync(Guid postId);
        Task<IEnumerable<Post>> SearchUserPostsAsync(string searchText, int limit);
        Task<IEnumerable<Post>> SearchHashtagsAsync(string tag, int limit);

        Task<IEnumerable<Post>> SearchPostsAsync(string searchText, int limit);
    }
}