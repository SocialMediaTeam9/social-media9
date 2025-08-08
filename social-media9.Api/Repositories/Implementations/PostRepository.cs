using System;

using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using social_media9.Api.Models;

using social_media9.Api.Data;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Dtos;
using Amazon.DynamoDBv2.DocumentModel;

namespace social_media9.Api.Repositories.Implementations
{
    public class PostRepository : IPostRepository
    {
        private readonly IDynamoDBContext _context;

        public PostRepository(IDynamoDBContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<Post>> GetPostsByUsernameAsync(string username, int limit = 20)
        {

            var queryConfig = new QueryOperationConfig
            {
                IndexName = "GSI1",
                Filter = new QueryFilter("GSI1PK", QueryOperator.Equal, $"USER#{username}"),
                KeyExpression = new Expression
                {
                    ExpressionStatement = "begins_with(GSI1SK, :v_sk)",
                    ExpressionAttributeValues = { [":v_sk"] = "POST#" }
                },
                Limit = limit,
                BackwardSearch = true
            };

            var query = _context.FromQueryAsync<Post>(queryConfig);
            var posts = await query.GetNextSetAsync();

            return posts.Take(limit);
        }

        public async Task AddAsync(Post post)
        {
            await _context.SaveAsync(post);
        }

        public async Task<IEnumerable<Post>> GetAllAsync()
        {
            var conditions = new List<ScanCondition>();
            var posts = await _context.ScanAsync<Post>(conditions).GetRemainingAsync();
            return posts;
        }

        public async Task<Post> GetByIdAsync(Guid postId)
        {
            return await _context.LoadAsync<Post>($"POST#{postId}", $"POST#{postId}");
        }

        public async Task<IEnumerable<LikeEntity>> GetLikesAsync(string postId, int limit = 20)
        {
            var queryConfig = new QueryOperationConfig
            {
                Filter = new QueryFilter("PK", QueryOperator.Equal, $"POST#{postId}"),
                KeyExpression = new Expression
                {
                    ExpressionStatement = "begins_with(SK, :v_sk)",
                    ExpressionAttributeValues = { [":v_sk"] = "LIKE#" }
                },
                Limit = limit,
                BackwardSearch = true
            };


            var search = _context.FromQueryAsync<LikeEntity>(queryConfig);
            return await search.GetNextSetAsync();
        }


        public async Task<bool> LikeAsync(Guid postId, Guid userId)
        {
            var like = new Like
            {
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _context.SaveAsync(like);
            return true;
        }

        public Task<bool> UnlikeAsync(Guid postId, Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<Post> GetByIdAsync(string postId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Comment>> GetCommentsAsync(string postId, int limit = 50)
        {

            var queryConfig = new QueryOperationConfig
            {
                Filter = new QueryFilter("PK", QueryOperator.Equal, $"POST#{postId}"),

                KeyExpression = new Expression
                {
                    ExpressionStatement = "begins_with(SK, :v_sk)",
                    ExpressionAttributeValues = { [":v_sk"] = "COMMENT#" }
                },

                Limit = limit,


                BackwardSearch = false
            };

            var search = _context.FromQueryAsync<Comment>(queryConfig);
            return await search.GetNextSetAsync();
        }

        public async Task<IEnumerable<Post>> SearchUserPostsAsync(string searchText, int limit)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return Enumerable.Empty<Post>();

            var filter = new ScanFilter();
            filter.AddCondition("Content", ScanOperator.Contains, searchText);

            var scanConfig = new ScanOperationConfig { Filter = filter };

            var search = _context.FromScanAsync<Post>(scanConfig);
            var results = new List<Post>();
            do
            {
                var page = await search.GetNextSetAsync();
                results.AddRange(page.Where(p =>
                    p.Content?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0));

            } while (!search.IsDone && results.Count < limit);

            return results.OrderByDescending(p => p.CreatedAt).Take(limit);
        }

        public async Task<IEnumerable<Post>> SearchHashtagsAsync(string tag, int limit)
        {
            var cleanTag = tag.TrimStart('#');
            if (string.IsNullOrWhiteSpace(cleanTag)) return Enumerable.Empty<Post>();

            return await SearchUserPostsAsync("#" + cleanTag, limit);
        }

        public async Task<IEnumerable<Post>> SearchPostsAsync(string searchText, int limit)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return Enumerable.Empty<Post>();

            var filter = new ScanFilter();
            filter.AddCondition("Content", ScanOperator.Contains, searchText);

            var scanConfig = new ScanOperationConfig { Filter = filter };

            var search = _context.FromScanAsync<Post>(scanConfig);
            var results = new List<Post>();
            do
            {
                var page = await search.GetNextSetAsync();
                results.AddRange(page.Where(p =>
                    p.Content?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0));

            } while (!search.IsDone && results.Count < limit);

            return results.OrderByDescending(p => p.CreatedAt).Take(limit);
        }
        
    }
}
