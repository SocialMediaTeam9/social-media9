using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using social_media9.Api.Models;
using social_media9.Api.Models.DynamoDb;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Amazon.DynamoDBv2;

namespace social_media9.Api.Services.DynamoDB;

public class DynamoDbService
{
    private readonly IDynamoDBContext _dbContext;
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly IConfiguration _config;
    private readonly string _tableName;
    private readonly ILogger<DynamoDbService> _logger;

    public DynamoDbService(IAmazonDynamoDB dynamoDbClient, IDynamoDBContext dbContext, IConfiguration config, ILogger<DynamoDbService> logger)
    {
        _dynamoDbClient = dynamoDbClient;
        _dbContext = dbContext;
        _config = config;
        _tableName = config["DynamoDbTableName"] ?? "nexusphere-mvp-main-table";
        _logger = logger;
    }

    #region User Methods

    /// <summary>
    /// Creates a new user profile and a username uniqueness lock in a single transaction.
    /// </summary>
    public async Task<bool> CreateUserAsync(User newUser)
    {
        var usernameLock = UsernameEntity.Create(newUser.Username, newUser.UserId);

        var transactionRequest = new TransactWriteItemsRequest
        {
            TransactItems = new List<TransactWriteItem>
                {
                    new TransactWriteItem
                    {
                        Put = new Put
                        {
                            TableName = _tableName,
                            Item = _dbContext.ToDocument(newUser).ToAttributeMap(),
                            ConditionExpression = "attribute_not_exists(PK)"
                        }
                    },
                    new TransactWriteItem
                    {
                        Put = new Put
                        {
                            TableName = _tableName,
                            Item = _dbContext.ToDocument(usernameLock).ToAttributeMap(),
                            ConditionExpression = "attribute_not_exists(PK)"
                        }
                    }
                }
        };


        try
        {
            await _dynamoDbClient.TransactWriteItemsAsync(transactionRequest);
            return true;
        }
        catch (TransactionCanceledException ex)
        {
            _logger.LogWarning(ex, "Transaction failed to create user {Username}. It likely already exists.", newUser.Username);
            return false;
        }
    }

    /// <summary>
    /// Retrieves a full user profile by their username.
    /// </summary>
    public async Task<User?> GetUserProfileByUsernameAsync(string username)
    {
        return await _dbContext.LoadAsync<User>($"USER#{username}", "METADATA");
    }

    /// <summary>
    /// Retrieves a lightweight summary of a user, useful for denormalization.
    /// </summary>
    public async Task<UserSummary?> GetUserSummaryAsync(string username)
    {
        var user = await GetUserProfileByUsernameAsync(username);
        if (user == null) return null;

        var domain = _config["DomainName"];
        return new UserSummary(
            "",
            Username: user.Username,
            ActorUrl: $"https://fed.{domain}/users/{user.Username}",
            ProfilePictureUrl: user.ProfilePictureUrl
        );
    }

    #endregion

    #region Follow Methods

    public async Task<bool> ProcessFollowActivityAsync(string followerActorUrl, string followedUsername)
    {
        var followerUsername = followerActorUrl.Split('/').Last();
        var followerSummary = new UserSummary("", followerUsername, followerActorUrl, null);
        var followedSummary = await GetUserSummaryAsync(followedUsername);
        if (followedSummary == null) return false;
        return await CreateFollowAndIncrementCountsAsync(followerSummary, followedSummary);
    }


    public async Task<bool> CreateFollowAndIncrementCountsAsync(UserSummary follower, UserSummary following)
    {
        var followEntity = Follow.Create(follower, following);

        var transactionRequest = new TransactWriteItemsRequest
        {
            TransactItems = new List<TransactWriteItem>
                {
                    new TransactWriteItem
                    {
                        Put = new Put
                        {
                            TableName = _tableName,
                            Item = _dbContext.ToDocument(followEntity).ToAttributeMap(),
                            ConditionExpression = "attribute_not_exists(PK)"
                        }
                    },
                    new TransactWriteItem
                    {
                        Update = CreateUpdateCountRequest($"USER#{follower.Username}", "METADATA", "FollowingCount", 1)
                    },
                    new TransactWriteItem
                    {
                        Update = CreateUpdateCountRequest($"USER#{following.Username}", "METADATA", "FollowersCount", 1)
                    }
                }
        };

        try
        {
            await _dynamoDbClient.TransactWriteItemsAsync(transactionRequest);
            return true;
        }
        catch (TransactionCanceledException ex)
        {
            _logger.LogWarning(ex, "Transaction to follow user {FollowingUsername} by {FollowerUsername} failed.", following.Username, follower.Username);
            return false;
        }
    }

    /// <summary>
    /// Retrieves the list of entities representing the followers of a user.
    /// Uses the GSI for an efficient reverse lookup.
    /// </summary>
    public async Task<List<Follow>> GetFollowersAsync(string username)
    {
        var config = new DynamoDBOperationConfig
        {
            IndexName = "GSI1"
        };
        var query = _dbContext.QueryAsync<Follow>($"USER#{username}", QueryOperator.BeginsWith, new[] { "FOLLOWED_BY#" });
        return await query.GetNextSetAsync();
    }


    public async Task<bool> ProcessUnfollowActivityAsync(string followerActorUrl, string followedUsername)
    {
        var followerUsername = followerActorUrl.Split('/').Last();
        var transactionRequest = new TransactWriteItemsRequest
        {
            TransactItems = new List<TransactWriteItem>
                {
                    new() { Delete = new Delete { TableName = _tableName, Key = new Dictionary<string, AttributeValue> { { "PK", new AttributeValue($"USER#{followerUsername}") }, { "SK", new AttributeValue($"FOLLOWS#{followedUsername}") } } } },
                    new() { Update = CreateUpdateCountRequest($"USER#{followerUsername}", "METADATA", "FollowingCount", -1) },
                    new() { Update = CreateUpdateCountRequest($"USER#{followedUsername}", "METADATA", "FollowersCount", -1) }
                }
        };
        try
        {
            await _dynamoDbClient.TransactWriteItemsAsync(transactionRequest);
            _logger.LogInformation("User {FollowerUsername} successfully unfollowed {FollowingUsername}", followerUsername, followedUsername);
            return true;
        }
        catch (TransactionCanceledException ex)
        {
            _logger.LogWarning(ex, "Transaction to unfollow user {FollowingUsername} by {FollowerUsername} failed.", followedUsername, followerUsername);
            return false;
        }
    }

    public async Task<bool> DeleteFollowAndDecrementCountsAsync(UserSummary follower, UserSummary unfollowed)
    {
        var transactionRequest = new TransactWriteItemsRequest
        {
            TransactItems = new List<TransactWriteItem>
            {
                new() {
                    Delete = new Delete
                    {
                        TableName = _tableName,
                        Key = new Dictionary<string, AttributeValue>
                        {
                            { "PK", new AttributeValue($"USER#{follower.Username}") },
                            { "SK", new AttributeValue($"FOLLOWS#{unfollowed.Username}") }
                        },
                        ConditionExpression = "attribute_exists(PK)"
                    }
                },
                new() {
                    Update = CreateUpdateCountRequest($"USER#{follower.Username}", "METADATA", "FollowingCount", -1)
                },
                new() {
                    Update = CreateUpdateCountRequest($"USER#{unfollowed.Username}", "METADATA", "FollowersCount", -1)
                }
            }
        };

        try
        {
            await _dynamoDbClient.TransactWriteItemsAsync(transactionRequest);
            _logger.LogInformation("User {FollowerUsername} successfully unfollowed {UnfollowedUsername}", follower.Username, unfollowed.Username);
            return true;
        }
        catch (TransactionCanceledException ex)
        {
            _logger.LogWarning(ex, "Transaction to unfollow user {UnfollowedUsername} by {FollowerUsername} failed.", unfollowed.Username, follower.Username);
            return false;
        }
    }

    public async Task<List<Follow>> GetFollowingAsync(string username)
    {
        var query = _dbContext.QueryAsync<Follow>($"USER#{username}", QueryOperator.BeginsWith, new[] { "FOLLOWS#" });
        return await query.GetNextSetAsync();
    }

    #endregion

    #region Post Methods

    /// <summary>
    /// Creates a single new post item.
    /// </summary>
    public async Task<bool> CreatePostAsync(Post post)
    {
        try
        {
            await _dbContext.SaveAsync(post);
            return true;
        }
        catch (ConditionalCheckFailedException)
        {
            // Console.WriteLine($"Create post failed due to a key collision for PK={post.PK}");
            return false;
        }
    }

    /// <summary>
    /// Retrieves a single post by its ID (the ULID part).
    /// </summary>
    public async Task<Post?> GetPostByIdAsync(string postId)
    {
        return await _dbContext.LoadAsync<Post>($"POST#{postId}", $"POST#{postId}");
    }

    public async Task<bool> LikePostAsync(string postId, UserSummary liker)
    {
        var likeEntity = new LikeEntity
        {
            PK = $"POST#{postId}",
            SK = $"LIKE#{liker.ActorUrl}", // Use ActorUrl for global uniqueness
            GSI1PK = $"USER#{liker.Username}",
            GSI1SK = $"LIKE#{postId}",
            LikerUsername = liker.Username,
            CreatedAt = DateTime.UtcNow
        };

        var transactionRequest = new TransactWriteItemsRequest
        {
            TransactItems = new List<TransactWriteItem>
            {
                new() { Put = new Put { TableName = _tableName, Item = _dbContext.ToDocument(likeEntity).ToAttributeMap(), ConditionExpression = "attribute_not_exists(PK)" } },
                new() { Update = CreateUpdateCountRequest($"POST#{postId}", $"POST#{postId}", "LikeCount", 1) }
            }
        };

        try
        {
            await _dynamoDbClient.TransactWriteItemsAsync(transactionRequest);
            _logger.LogInformation("User {Username} successfully liked post {PostId}", liker.Username, postId);
            return true;
        }
        catch (TransactionCanceledException ex)
        {
            _logger.LogWarning(ex, "Transaction to like post {PostId} failed. It may not exist or was already liked.", postId);
            return false;
        }
    }

    public async Task<bool> CreateCommentAsync(Comment comment)
    {

        var transactionRequest = new TransactWriteItemsRequest
        {
            TransactItems = new List<TransactWriteItem>
                {
                    // Item 1: The new Comment
                    new TransactWriteItem
                    {
                        Put = new Put
                        {
                            TableName = _tableName,
                            Item = _dbContext.ToDocument(comment).ToAttributeMap(),
                            ConditionExpression = "attribute_not_exists(PK)"
                        }
                    },
                    // Item 2: Increment the parent post's CommentCount
                    new TransactWriteItem
                    {
                        Update = CreateUpdateCountRequest(comment.PK, comment.PK, "CommentCount", 1)
                    }
                }
        };

        // var transaction = _dbContext.CreateTransactWrite();
        // transaction.AddPut(comment, new DynamoDBOperationConfig
        // {
        //     ConditionalExpression = new Expression { ExpressionStatement = "attribute_not_exists(PK)" }
        // });
        // transaction.AddUpdate(CreateUpdatePostCountRequest(comment.PK, "CommentCount", 1));

        try
        {
            await _dynamoDbClient.TransactWriteItemsAsync(transactionRequest);
            return true;
        }
        catch (TransactionCanceledException)
        {
            return false;
        }
    }

    public async Task<List<Comment>> GetCommentsForPostAsync(string postId)
    {
        var query = _dbContext.QueryAsync<Comment>($"POST#{postId}", QueryOperator.BeginsWith, new[] { "COMMENT#" });
        return await query.GetNextSetAsync();
    }

    #endregion

    #region Timeline Methods

    public async Task PopulateTimelinesAsync(Post post, List<string> followerUsernames)
    {
        if (!followerUsernames.Any()) return;

        var batch = _dbContext.CreateBatchWrite<BaseEntity>();
        foreach (var username in followerUsernames)
        {
            var timelineItem = new TimelineItemEntity
            {
                PK = $"TIMELINE#{username}",
                SK = $"NOTE#{post.SK.Replace("POST#", "")}",
                AuthorUsername = post.AuthorUsername,
                PostContent = post.Content,
                AttachmentUrls = post.Attachments,
                CreatedAt = post.CreatedAt
            };
            batch.AddPutItem(timelineItem);
        }
        await batch.ExecuteAsync();
    }

    public async Task<(List<TimelineItemEntity> Items, string? NextToken)> GetTimelineAsync(
        string username,
        int pageSize = 20,
        string? paginationToken = null)
    {
        var pk = $"TIMELINE#{username}";

        var queryConfig = new QueryOperationConfig
        {
            Limit = pageSize,
           
            // BackwardSearch = false
            Filter = new QueryFilter("PK", QueryOperator.Equal, pk)
        };

        if (!string.IsNullOrEmpty(paginationToken))
        {
            queryConfig.PaginationToken = paginationToken;
        }

        var query = _dbContext.FromQueryAsync<TimelineItemEntity>(queryConfig);

        var items = await query.GetNextSetAsync();

        string? nextToken = query.PaginationToken;

        return (items, nextToken);
    }

    #endregion

    #region Private Helpers

    private Update CreateUpdateUserCountRequest(string username, string attributeName, int incrementBy)
    {
        var userEntity = new User { PK = $"USER#{username}", SK = "METADATA" };
        var doc = _dbContext.ToDocument(userEntity);
        return new Update
        {
            TableName = _tableName,
            Key = doc.ToAttributeMap(),
            UpdateExpression = $"ADD {attributeName} :inc",
            ExpressionAttributeValues = new Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>
            { { ":inc", new Amazon.DynamoDBv2.Model.AttributeValue { N = incrementBy.ToString() } } },
            ConditionExpression = "attribute_exists(PK)"
        };
    }

    private Update CreateUpdatePostCountRequest(string postPk, string attributeName, int incrementBy)
    {
        var postEntity = new Post { PK = postPk, SK = postPk };
        var doc = _dbContext.ToDocument(postEntity);
        return new Update
        {
            TableName = _tableName,
            Key = doc.ToAttributeMap(),
            UpdateExpression = $"ADD {attributeName} :inc",
            ExpressionAttributeValues = new Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>
            { { ":inc", new Amazon.DynamoDBv2.Model.AttributeValue { N = incrementBy.ToString() } } },
            ConditionExpression = "attribute_exists(PK)"
        };
    }

    private Update CreateUpdateCountRequest(string pk, string sk, string attributeName, int incrementBy)
    {
        return new Update
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
                {
                    { "PK", new AttributeValue(pk) },
                    { "SK", new AttributeValue(sk) }
                },
            UpdateExpression = $"ADD {attributeName} :inc",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    { { ":inc", new AttributeValue { N = incrementBy.ToString() } } },
            ConditionExpression = "attribute_exists(PK)"
        };
    }

    #endregion

    #region Activity Methods

    // ... (ProcessFollowActivityAsync and ProcessUnfollowActivityAsync as before)

    /// <summary>
    /// Processes a 'Like' activity by creating a Like record and incrementing the post's like count.
    /// </summary>
    public async Task<bool> ProcessLikeActivityAsync(string likerActorUrl, string likedPostId)
    {
        var likerUsername = likerActorUrl.Split('/').Last();
        var likeEntity = new LikeEntity
        {
            PK = $"POST#{likedPostId}",
            SK = $"LIKE#{likerActorUrl}",
            GSI1PK = $"USER#{likerUsername}",
            GSI1SK = $"LIKE#{likedPostId}",
            LikerUsername = likerUsername,
            CreatedAt = DateTime.UtcNow
        };
        var transactionRequest = new TransactWriteItemsRequest
        {
            TransactItems = new List<TransactWriteItem>
                {
                    new() { Put = new Put { TableName = _tableName, Item = _dbContext.ToDocument(likeEntity).ToAttributeMap(), ConditionExpression = "attribute_not_exists(PK)" } },
                    new() { Update = CreateUpdateCountRequest($"POST#{likedPostId}", $"POST#{likedPostId}", "LikeCount", 1) }
                }
        };

        try
        {
            await _dynamoDbClient.TransactWriteItemsAsync(transactionRequest);
            _logger.LogInformation("Recorded Like on post {PostId} by actor {ActorUrl}", likedPostId, likerActorUrl);
            return true;
        }
        catch (TransactionCanceledException ex)
        {
            _logger.LogWarning(ex, "Transaction to like post {PostId} failed. It may not exist or was already liked.", likedPostId);
            return false;
        }
    }

    /// <summary>
    /// Processes an 'Announce' (boost) activity.
    /// It creates a Boost record, increments the count, AND returns the original post
    /// so the worker can re-deliver it to the booster's followers.
    /// </summary>
    public async Task<Post?> ProcessBoostActivityAsync(string boosterActorUrl, string boostedPostId)
    {
        var boosterUsername = boosterActorUrl.Split('/').Last();

        var boostEntity = new BoostEntity
        {
            PK = $"POST#{boostedPostId}",
            SK = $"BOOST#{boosterActorUrl}",
            GSI1PK = $"USER#{boosterUsername}",
            GSI1SK = $"BOOST#{boostedPostId}",
            BoosterUsername = boosterUsername,
            CreatedAt = DateTime.UtcNow
        };
        var transactionRequest = new TransactWriteItemsRequest
        {
            TransactItems = new List<TransactWriteItem>
                {
                    new() { Put = new Put { TableName = _tableName, Item = _dbContext.ToDocument(boostEntity).ToAttributeMap(), ConditionExpression = "attribute_not_exists(PK)" } },
                    new() { Update = CreateUpdateCountRequest($"POST#{boostedPostId}", $"POST#{boostedPostId}", "BoostCount", 1) }
                }
        };
        try
        {
            await _dynamoDbClient.TransactWriteItemsAsync(transactionRequest);
            _logger.LogInformation("Recorded Boost on post {PostId} by actor {ActorUrl}", boostedPostId, boosterActorUrl);
            return await GetPostByIdAsync(boostedPostId);
        }
        catch (TransactionCanceledException ex)
        {
            _logger.LogWarning(ex, "Transaction to boost post {PostId} failed. It may not exist or was already boosted.", boostedPostId);
            return null;
        }
    }

    #endregion
}

// --- Helper Extension Method ---
public static class DynamoDbContextExtensions
{
    // Helper to create the low-level Update object for incrementing counts.
    public static Update CreateUpdateItemRequestForUserCount(this IDynamoDBContext context, string username, string attributeName, int incrementBy)
    {
        var userEntity = new User { PK = $"USER#{username}", SK = "METADATA" };
        var doc = context.ToDocument(userEntity);

        var update = new Update
        {
            TableName = "nexusphere-mvp-main-table",
            Key = doc.ToAttributeMap(),
            UpdateExpression = $"ADD {attributeName} :inc",
            ExpressionAttributeValues = new Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>
            {
                { ":inc", new Amazon.DynamoDBv2.Model.AttributeValue { N = incrementBy.ToString() } }
            },
            ConditionExpression = "attribute_exists(PK)"
        };
        return update;
    }
}