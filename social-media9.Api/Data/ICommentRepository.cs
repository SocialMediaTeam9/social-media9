using social_media9.Api.Models;

namespace social_media9.Api.Data
{
    public interface ICommentRepository
    {
        Task SaveCommentAsync(Comment comment);
        Task<List<Comment>> GetCommentsByContentAsync(Guid postId);
        Task<bool> DeleteCommentAsync(Guid commentId, Guid postId);
        Task<bool> UpdateCommentAsync(Guid commentId, string newContent);
    }
}