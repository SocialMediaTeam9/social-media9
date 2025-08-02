using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2;
using social_media9.Api.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace social_media9.Api.Repositories.Implementations
{
    public class FollowRepository : IFollowRepository
    {
        private readonly IDynamoDBContext _context;
        private readonly IAmazonDynamoDB _dynamoDbClient; // For low-level updates
        private readonly DynamoDbSettings _settings;
        private readonly IUserRepository _userRepository; // To update user counts

        public FollowRepository(DynamoDbClientFactory clientFactory, IUserRepository userRepository)
        {
            _context = clientFactory.GetContext();
            _dynamoDbClient = clientFactory.GetClient();
            _settings = clientFactory.GetSettings();
            _userRepository = userRepository;
        }

        public async Task AddFollowAsync(string followerId, string followingId)
        {
            var existingFollow = await IsFollowingAsync(followerId, followingId);
            if (!existingFollow)
            {
                var follow = new Follow { FollowerId = followerId, FollowingId = followingId, CreatedAt = DateTime.UtcNow };
                await _context.SaveAsync(follow);

                // Update counts on User documents
                await IncrementFollowingCountAsync(followerId);
                await IncrementFollowersCountAsync(followingId);
            }
        }

        public async Task RemoveFollowAsync(string followerId, string followingId)
        {
            var isFollowing = await IsFollowingAsync(followerId, followingId);
            if (isFollowing)
            {
                await _context.DeleteAsync<Follow>(followerId, followingId);

                // Update counts on User documents
                await DecrementFollowingCountAsync(followerId);
                await DecrementFollowersCountAsync(followingId);
            }
        }

        public async Task<bool> IsFollowingAsync(string followerId, string followingId)
        {
           var config = new LoadConfig { ConsistentRead = false };
           var follow = await _context.LoadAsync<Follow>(followerId, followingId, config);

            return follow != null;
        }

        public async Task<IEnumerable<string>> GetFollowersAsync(string userId)
        {
            // Query by FollowingId (GSI needed for this if not using a separate table)
            // For now, assuming you'd query the Follows table where FollowingId is a GSI Hash Key
            // You would need to define a GSI on Follows table: 'FollowingId-index' with FollowingId as HashKey
            var queryFilter = new QueryFilter("FollowingId", QueryOperator.Equal, userId);
            var config = new QueryConfig { IndexName = "FollowingId-index" }; // Requires GSI
            var search = _context.QueryAsync<Follow>(userId, config);
            var results = await search.GetRemainingAsync();
            return results.Select(f => f.FollowerId).ToList();
        }

        public async Task<IEnumerable<string>> GetFollowingAsync(string userId)
        {
            // Query by FollowerId (which is the HashKey)
            var search = _context.QueryAsync<Follow>(userId);
            var results = await search.GetRemainingAsync();
            return results.Select(f => f.FollowingId).ToList();
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
    }
}
