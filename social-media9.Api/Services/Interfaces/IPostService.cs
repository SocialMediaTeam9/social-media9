using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using social_media9.Api.Dtos;
using social_media9.Api.Models;

namespace social_media9.Api.Services.Interfaces
{
    public interface IPostService
    {
        // Task<Guid> CreatePostAsync(CreatePostRequest request, Guid userId);
        // Task<IEnumerable<PostDTO>> GetPostsAsync();

        Task<IEnumerable<Post>> GetUserPostsAsync(String username);
        Task<Post?> GetPostByIdAsync(string postId);
        // Task<IEnumerable<PostDTO>> GetUserPostsAsync(string userId);
        Task<bool> LikePostAsync(string postId, string likerUsername);

        Task<Post?> CreateAndFederatePostAsync(string authorUsername, string content, List<string>? attachmentUrls);

        Task<IEnumerable<LikeEntity>> GetLikesAsync(string postId, int limit = 20);

        Task<Comment?> AddCommentAsync(string postId, string authorUsername, string content);

        Task<IEnumerable<Comment>> GetCommentsForPostAsync(string postId);
        // Task<IEnumerable<UserSummaryDTO>> GetPostLikesAsync(Guid postId);
        // Task<Guid> AddCommentAsync(Guid postId, AddCommentRequest request, Guid userId);
        //Task<IEnumerable<CommentDTO>> GetCommentsAsync(Guid postId);
    }
}