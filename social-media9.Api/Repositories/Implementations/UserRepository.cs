using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using social_media9.Api.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using social_media9.Api.Repositories.Interfaces;

namespace social_media9.Api.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly IDynamoDBContext _dbContext;

        public UserRepository(IDynamoDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _dbContext.LoadAsync<User>(userId);
        }

        public async Task<User?> GetUserByGoogleIdAsync(string googleId)
        {
            var queryConfig = new QueryOperationConfig
            {
                IndexName = "GoogleId-index",
                KeyExpression = new Expression
                {
                    ExpressionStatement = "GoogleId = :v_googleId",
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                    {
                        [":v_googleId"] = googleId
                    }
                }
            };

            var search = _dbContext.FromQueryAsync<User>(queryConfig);
            var results = await search.GetNextSetAsync();
            return results.FirstOrDefault();
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            var queryConfig = new QueryOperationConfig
            {
                IndexName = "Username-index",
                KeyExpression = new Expression
                {
                    ExpressionStatement = "Username = :v_username",
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                    {
                        [":v_username"] = username
                    }
                }
            };

            var search = _dbContext.FromQueryAsync<User>(queryConfig);
            var users = await search.GetNextSetAsync();
            return users.FirstOrDefault();
        }

        public async Task<User> AddUserAsync(User user)
        {
            await _dbContext.SaveAsync(user);
            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            await _dbContext.SaveAsync(user);
            return user;
        }

        public async Task DeleteUserAsync(string userId)
        {
            await _dbContext.DeleteAsync<User>(userId);
        }

        public async Task<bool> ExistsAsync(string userId)
        {
            var user = await _dbContext.LoadAsync<User>(userId);
            return user != null;
        }

        public async Task<IEnumerable<User>> GetUsersByIdsAsync(IEnumerable<string> ids)
        {
            var batch = _dbContext.CreateBatchGet<User>();
            foreach (var id in ids)
            {
                batch.AddKey(id);
            }
            await batch.ExecuteAsync();
            return batch.Results;
        }
    }
}
