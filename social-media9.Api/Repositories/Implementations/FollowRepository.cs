using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2;
using social_media9.Api.Models;
using social_media9.Api.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Services.DynamoDB;

namespace social_media9.Api.Repositories.Implementations
{
    /*public class FollowRepository : IFollowRepository
    {
        private readonly IDynamoDBContext _context;
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly DynamoDbSettings _settings;
        private readonly IUserRepository _userRepository;

        private readonly DynamoDbService _dbService;

        public FollowRepository(DynamoDbClientFactory clientFactory, IUserRepository userRepository, DynamoDbService dbService)
        {
            _context = clientFactory.GetContext();
            _dynamoDbClient = clientFactory.GetClient();
            _settings = clientFactory.GetSettings();
            _userRepository = userRepository;
            _dbService = dbService;
        }
        public async Task<bool> IsFollowingAsync(string followerId, string followingId)
        {
           
            var follower = await _userRepository.GetUserByIdAsync(followerId);
            var following = await _userRepository.GetUserByIdAsync(followingId);

            if (follower == null || following == null)
            {
                return false;
            }

            return await _dbService.IsFollowingAsync(follower.Username, following.Username);
        }

        public async Task<IEnumerable<Follow>> GetFollowersAsync(string username)
        {
            var queryConfig = new QueryOperationConfig
            {
                IndexName = "GSI1",
                Filter = new QueryFilter("GSI1PK", QueryOperator.Equal, $"USER#{username}"),
                KeyExpression = new Expression
                {
                    ExpressionStatement = "begins_with(GSI1SK, :v_sk)",
                    ExpressionAttributeValues = { [":v_sk"] = "FOLLOWED_BY#" }
                }
            };
            var search = _context.FromQueryAsync<Follow>(queryConfig);
            return await search.GetNextSetAsync();

        }

        public async Task<IEnumerable<Follow>> GetFollowingAsync(string username)
        {
            var queryConfig = new QueryOperationConfig
            {
                Filter = new QueryFilter("PK", QueryOperator.Equal, $"USER#{username}"),
                KeyExpression = new Expression
                {
                    ExpressionStatement = "begins_with(SK, :v_sk)",
                    ExpressionAttributeValues = { [":v_sk"] = "FOLLOWS#" }
                }
            };
            var search = _context.FromQueryAsync<Follow>(queryConfig);
            return await search.GetNextSetAsync();
        }

        // --- Methods to update denormalized counts on User table ---
        // These use low-level DynamoDB UpdateItem for atomic increments/decrements
        public async Task IncrementFollowersCountAsync(string userId)
        {
            var request = new Amazon.DynamoDBv2.Model.UpdateItemRequest
            {
                TableName = _settings.UsersTableName,
                Key = new Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>
                {
                    { "UserId", new Amazon.DynamoDBv2.Model.AttributeValue { S = userId } }
                },
                UpdateExpression = "SET FollowersCount = if_not_exists(FollowersCount, :start) + :val",
                ExpressionAttributeValues = new Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>
                {
                    { ":val", new Amazon.DynamoDBv2.Model.AttributeValue { N = "1" } },
                    { ":start", new Amazon.DynamoDBv2.Model.AttributeValue { N = "0" } }
                },
                ReturnValues = "UPDATED_NEW"
            };
            await _dynamoDbClient.UpdateItemAsync(request);
        }

        public async Task DecrementFollowersCountAsync(string userId)
        {
            var request = new Amazon.DynamoDBv2.Model.UpdateItemRequest
            {
                TableName = _settings.UsersTableName,
                Key = new Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>
                {
                    { "UserId", new Amazon.DynamoDBv2.Model.AttributeValue { S = userId } }
                },
                UpdateExpression = "SET FollowersCount = if_not_exists(FollowersCount, :start) - :val",
                ExpressionAttributeValues = new Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>
                {
                    { ":val", new Amazon.DynamoDBv2.Model.AttributeValue { N = "1" } },
                    { ":start", new Amazon.DynamoDBv2.Model.AttributeValue { N = "0" } }
                },
                ReturnValues = "UPDATED_NEW"
            };
            await _dynamoDbClient.UpdateItemAsync(request);
        }

        public async Task IncrementFollowingCountAsync(string userId)
        {
            var request = new Amazon.DynamoDBv2.Model.UpdateItemRequest
            {
                TableName = _settings.UsersTableName,
                Key = new Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>
                {
                    { "UserId", new Amazon.DynamoDBv2.Model.AttributeValue { S = userId } }
                },
                UpdateExpression = "SET FollowingCount = if_not_exists(FollowingCount, :start) + :val",
                ExpressionAttributeValues = new Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>
                {
                    { ":val", new Amazon.DynamoDBv2.Model.AttributeValue { N = "1" } },
                    { ":start", new Amazon.DynamoDBv2.Model.AttributeValue { N = "0" } }
                },
                ReturnValues = "UPDATED_NEW"
            };
            await _dynamoDbClient.UpdateItemAsync(request);
        }

        public async Task DecrementFollowingCountAsync(string userId)
        {
            var request = new Amazon.DynamoDBv2.Model.UpdateItemRequest
            {
                TableName = _settings.UsersTableName,
                Key = new Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>
                {
                    { "UserId", new Amazon.DynamoDBv2.Model.AttributeValue { S = userId } }
                },
                UpdateExpression = "SET FollowingCount = if_not_exists(FollowingCount, :start) - :val",
                ExpressionAttributeValues = new Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>
                {
                    { ":val", new Amazon.DynamoDBv2.Model.AttributeValue { N = "1" } },
                    { ":start", new Amazon.DynamoDBv2.Model.AttributeValue { N = "0" } }
                },
                ReturnValues = "UPDATED_NEW"
            };
            await _dynamoDbClient.UpdateItemAsync(request);
        }
    }*/
}
