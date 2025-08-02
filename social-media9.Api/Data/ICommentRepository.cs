using social_media9.Api.Models;

namespace social_media9.Api.Data
{
    public interface ICommentRepository
    {
        Task SaveCommentAsync(Comment comment);
        Task<List<Comment>> GetCommentsByContentAsync(string contentId);
        Task DeleteCommentAsync(string commentId, string contentId);
        Task<bool> UpdateCommentAsync(string contentId, string commentId, string newContent);
    }
}