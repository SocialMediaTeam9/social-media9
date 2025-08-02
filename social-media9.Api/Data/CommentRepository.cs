using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using social_media9.Api.Data;
using social_media9.Api.Models;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;


namespace social_media9.Api.Data
{
    public class CommentRepository : ICommentRepository
    {
        private readonly IDynamoDBContext _context;
        private readonly string _tableName;

        public CommentRepository(DynamoDbClientFactory factory)
        {
            _context = factory.GetContext();
            _tableName = factory.GetSettings().CommentsTableName;
        }

        public async Task SaveCommentAsync(Comment comment)
        {
            await _context.SaveAsync(comment);
        }

        public async Task<Comment?> GetCommentByIdAsync(string commentId)
        {
            return await _context.LoadAsync<Comment>(commentId);
        }

        public async Task<bool> DeleteCommentAsync(string commentId, string contentId)
        {
            try
            {
                await _context.DeleteAsync<Comment>(contentId, commentId);
                return true;
            }
            catch (Exception ex)
            {
                
                return false;
            }
        }

        public async Task<bool> UpdateCommentAsync(string commentId, string newContent)
        {
            // Scan for comment with given CommentId
            var scanConditions = new List<ScanCondition>
            {
                new ScanCondition("CommentId", ScanOperator.Equal, commentId)
            };

            var search = _context.ScanAsync<Comment>(scanConditions);
            var matches = await search.GetRemainingAsync();

            var comment = matches.FirstOrDefault();
            if (comment == null)
                return false;

            comment.Text = newContent;

            await _context.SaveAsync(comment);
            return true;
        }


        public async Task<List<Comment>> GetCommentsByContentAsync(string contentId)
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("ContentId", ScanOperator.Equal, contentId)
            };

            var search = _context.QueryAsync<Comment>(contentId);
            var results = await search.GetRemainingAsync();
            return results;
        }
    }
}