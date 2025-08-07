using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using social_media9.Api.Data;
using social_media9.Api.Models;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using social_media9.Api.Repositories.Interfaces;


namespace social_media9.Api.Repositories.Implementations
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

        public async Task<Comment?> GetCommentByIdAsync(Guid commentId)
        {
            return await _context.LoadAsync<Comment>(commentId.ToString());
        }

        public async Task<bool> DeleteCommentAsync(Guid commentId, Guid PostId)
        {
            try
            {
                await _context.DeleteAsync<Comment>(PostId.ToString(), commentId.ToString());
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        public async Task<bool> UpdateCommentAsync(Guid commentId, string newContent)
        {
            // Scan for comment with given CommentId
            var scanConditions = new List<ScanCondition>
            {
                new ScanCondition("CommentId", ScanOperator.Equal, commentId.ToString())
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


        public async Task<List<Comment>> GetCommentsByContentAsync(Guid postId)
        {
            var search = _context.QueryAsync<Comment>(postId.ToString());
            var results = await search.GetRemainingAsync();
            return results;
        }

        public async Task<Comment?> GetCommentByIdAsync(Guid commentId)
        {
            var search = _context.QueryAsync<Comment>(commentId.ToString());
            var results = await search.GetRemainingAsync();
            return results.FirstOrDefault();
        }
    }
}
