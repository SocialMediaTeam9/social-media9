using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using social_media9.Api.Dtos;

namespace social_media9.Api.Services.Interfaces
{
    public interface IPostService
    {
        Task<Guid> CreatePostAsync(CreatePostRequest request, Guid userId);
        Task<IEnumerable<PostDTO>> GetPostsAsync();
        Task<PostDTO> GetPostAsync(Guid postId);
        Task<IEnumerable<PostDTO>> GetUserPostsAsync(Guid userId);
        Task<bool> LikePostAsync(Guid postId, Guid userId);
        // Task<IEnumerable<UserSummaryDTO>> GetPostLikesAsync(Guid postId);
       // Task<Guid> AddCommentAsync(Guid postId, AddCommentRequest request, Guid userId);
        //Task<IEnumerable<CommentDTO>> GetCommentsAsync(Guid postId);
    }
}