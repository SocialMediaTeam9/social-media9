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
        private readonly IConfiguration _config;

        public UserRepository(IDynamoDBContext dbContext, IConfiguration config)
        {
            _dbContext = dbContext;
            _config = config;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _dbContext.LoadAsync<User>($"USER#{username}", "METADATA");
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {

            var config = new QueryOperationConfig
            {
                IndexName = "UserId-index",
                Filter = new QueryFilter("UserId", QueryOperator.Equal, userId),
                Limit = 1
            };

            var search = _dbContext.QueryAsync<User>(config);
            var users = await search.GetNextSetAsync();
            return users.FirstOrDefault();

        }

        public async Task<User?> GetUserByGoogleIdAsync(string googleId)
        {

            if (string.IsNullOrEmpty(googleId))
            {
                return null;
            }

            var queryConfig = new QueryOperationConfig
            {
                IndexName = "GoogleId-index",
                Filter = new QueryFilter("GoogleId", QueryOperator.Equal, googleId)
                // KeyExpression = new Expression
                // {
                //     ExpressionStatement = "GoogleId = :v_googleId",
                //     ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                //     {
                //         [":v_googleId"] = googleId
                //     }
                // }
            };

            var search = _dbContext.FromQueryAsync<User>(queryConfig);
            var results = await search.GetNextSetAsync();
            return results.FirstOrDefault();
        }

        // public async Task<User?> GetUserByUsernameAsync(string username)
        // {
        //     var queryConfig = new QueryOperationConfig
        //     {
        //         IndexName = "Username-index",
        //         KeyExpression = new Expression
        //         {
        //             ExpressionStatement = "Username = :v_username",
        //             ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
        //             {
        //                 [":v_username"] = username
        //             }
        //         }
        //     };

        //     var search = _dbContext.FromQueryAsync<User>(queryConfig);
        //     var users = await search.GetNextSetAsync();
        //     return users.FirstOrDefault();
        // }

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

        public async Task<IEnumerable<User>> GetUsersByIdsAsync(IEnumerable<string> userIds)
        {
            var distinctIds = userIds.Distinct().ToList();
            if (!distinctIds.Any())
            {
                return new List<User>();
            }

            var lookupTasks = distinctIds.Select(id => GetUserByIdAsync(id)).ToList();
            var usersFound = await Task.WhenAll(lookupTasks);

            var batch = _dbContext.CreateBatchGet<User>();

            foreach (var user in usersFound.Where(u => u != null))
            {
                batch.AddKey(user!.PK, user.SK);
            }

            if (batch.TotalKeys == 0)
            {
                return new List<User>();
            }

            await batch.ExecuteAsync();
            return batch.Results;
        }

        public async Task<UserSummary?> GetUserSummaryAsync(string username)
        {

            var user = await GetUserByUsernameAsync(username);

            if (user == null)
            {
                return null;
            }


            var domain = _config["DomainName"];
            if (string.IsNullOrEmpty(domain))
            {

                throw new InvalidOperationException("DomainName is not configured in app settings.");
            }

            return new UserSummary(
                "",
                Username: user.Username,
                ActorUrl: $"https://fed.{domain}/users/{user.Username}",
                ProfilePictureUrl: user.ProfilePictureUrl
            );
        }
    }
}
