using social_media9.Api.Models;

namespace social_media9.Api.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Task SaveCommentAsync(Comment comment);
        Task<List<Comment>> GetCommentsByContentAsync(Guid postId);
        Task<Comment?> GetCommentByIdAsync(Guid commentId);
        Task<bool> DeleteCommentAsync(Guid commentId, Guid postId);
        Task<bool> UpdateCommentAsync(Guid commentId, string newContent);
    }
}