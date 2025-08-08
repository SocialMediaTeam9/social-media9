using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using social_media9.Api.Models;
using social_media9.Api.Services.DynamoDB;
using social_media9.Api.Services.Interfaces;

public class SqsWorkerService : BackgroundService
{
    private readonly ILogger<SqsWorkerService> _logger;
    private readonly IAmazonSQS _sqsClient;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string? _queueUrl;
    private readonly IConfiguration _config;

    public SqsWorkerService(
       ILogger<SqsWorkerService> logger,
       IAmazonSQS sqsClient,
       IConfiguration config,
       IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _sqsClient = sqsClient;
        _scopeFactory = scopeFactory;
        _queueUrl = config["Aws:InboundSqsQueueUrl"];

        if (string.IsNullOrEmpty(_queueUrl))
        {
            throw new InvalidOperationException("Configuration value for 'Aws:InboundSqsQueueUrl' is missing or empty. The SQS worker cannot start.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SQS Worker Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var receiveRequest = new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 20
                };

                var result = await _sqsClient.ReceiveMessageAsync(receiveRequest, stoppingToken);
                if (result?.Messages != null)
                {
                    foreach (var message in result.Messages)
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbService = scope.ServiceProvider.GetRequiredService<DynamoDbService>();
                            await ProcessMessageAsync(message, scope.ServiceProvider);
                        }


                        await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, stoppingToken);
                    }
                }


            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in the SQS worker loop.");
                await Task.Delay(5000, stoppingToken);
            }
        }

        _logger.LogInformation("SQS Worker Service is stopping.");
    }

    private async Task ProcessMessageAsync(Message message, IServiceProvider serviceProvider)
    {
        _logger.LogInformation("--- RAW SQS MESSAGE BODY ---\n{MessageBody}\n--- END RAW BODY ---", message.Body);
        _logger.LogInformation("Processing SQS message ID: {MessageId}", message.MessageId);

        var activity = JsonDocument.Parse(message.Body).RootElement;

        var activityType = activity.TryGetProperty("type", out var type) ? type.GetString() : null;
        var actorUrl = activity.TryGetProperty("actor", out var actor) ? actor.GetString() : null;

        if (string.IsNullOrEmpty(activityType) || string.IsNullOrEmpty(actorUrl))
        {
            _logger.LogWarning("Received activity with missing type or actor.");
            return;
        }

        var dbService = serviceProvider.GetRequiredService<DynamoDbService>();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();



        switch (activityType)
        {
            case "Create":
                await HandleCreateActivityAsync(activity, dbService);
                break;
            case "Follow":
                var followTargetUrl = activity.TryGetProperty("object", out var obj) ? obj.GetString() : null;
                if (string.IsNullOrEmpty(followTargetUrl)) break;

                var followedUsername = ExtractUsernameFromActorUrl(followTargetUrl);
                var followerUsername = ExtractUsernameFromActorUrl(actorUrl);
                _logger.LogInformation("Processing INBOUND FOLLOW from {Follower} to {Followed}", actorUrl, followedUsername);

                bool isAlreadyFollowing = await dbService.IsFollowingAsync(followerUsername, followedUsername);

                if (isAlreadyFollowing)
                {
                    _logger.LogInformation("Follow relationship from {Follower} to {Followed} already exists. Skipping.", actorUrl, followedUsername);
                    // We don't send back another Accept. We just acknowledge the message is handled.
                    break; // Exit the case successfully.
                }

                var success = await dbService.ProcessFollowActivityAsync(actorUrl, followedUsername);

                if (success)
                {
                    _logger.LogInformation("Follow for {Followed} successful. Sending Accept activity back to {Follower}.", followedUsername, actorUrl);

                    var followedUserEntity = await dbService.GetUserProfileByUsernameAsync(followedUsername);
                    if (followedUserEntity == null || string.IsNullOrEmpty(followedUserEntity.PrivateKeyPem))
                    {
                        _logger.LogError("Could not find local user {Username} or they are missing a private key to sign the Accept activity.", followedUsername);
                        break;
                    }

                    var acceptPayload = new { type = "Accept", @object = activity.Deserialize<object>() };
                    var activityJson = BuildAcceptActivityJson(followedUserEntity.ActorUrl, acceptPayload, actorUrl);
                    var activityDoc = JsonDocument.Parse(activityJson);

                    var httpClient = httpClientFactory.CreateClient("FederationClient");
                    var deliveryService = new ActivityPubService(httpClient, followedUserEntity.ActorUrl, followedUserEntity.PrivateKeyPem, _config);

                    var targetInbox = $"{actorUrl}/inbox";
                    await deliveryService.DeliverActivityAsync(targetInbox, activityDoc);
                }
                break;
            case "Undo":
                if (activity.TryGetProperty("object", out var objectToUndo) &&
                    objectToUndo.TryGetProperty("type", out var typeToUndo) &&
                    typeToUndo.GetString() == "Follow")
                {
                    var unfollowTarget = objectToUndo.GetProperty("object").GetString();
                    if (unfollowTarget != null)
                    {
                        await dbService.ProcessUnfollowActivityAsync(actorUrl, ExtractUsernameFromActorUrl(unfollowTarget));
                    }
                }
                break;
            case "Like":
                var likedObjectUrl = activity.GetProperty("object").GetString();
                if (likedObjectUrl != null)
                {
                    await dbService.ProcessLikeActivityAsync(actorUrl, ExtractPostIdFromUrl(likedObjectUrl));
                }
                break;
            case "Announce":
                var boostedObjectUrl = activity.GetProperty("object").GetString();
                if (boostedObjectUrl != null)
                {
                    var boostedPostId = ExtractPostIdFromUrl(boostedObjectUrl);
                    var originalPost = await dbService.ProcessBoostActivityAsync(actorUrl, boostedPostId);

                    if (originalPost != null)
                    {
                        var boosterUsername = ExtractUsernameFromActorUrl(actorUrl);
                        var (boosterFollowers, nextToken) = await dbService.GetFollowersAsync(boosterUsername);
                        var followerUsernames = boosterFollowers.Select(f => f.FollowerInfo.Username).ToList();


                        await dbService.PopulateTimelinesAsync(originalPost, followerUsernames);
                    }
                }
                break;
            default:
                _logger.LogWarning("Received unknown activity type: {ActivityType}", activityType);
                break;

        }
    }

    private string BuildAcceptActivityJson(string actorUrl, object payload, string recipientActorUrl)
    {
        var domain = _config["DomainName"];
        var activityId = $"https://{domain}/users/{actorUrl.Split('/').Last()}/activities/{Ulid.NewUlid()}";

        var jsonObject = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(payload));
        jsonObject["@context"] = "https://www.w3.org/ns/activitystreams";
        jsonObject["id"] = activityId;
        jsonObject["actor"] = actorUrl;
        jsonObject["to"] = new[] { recipientActorUrl };

        return JsonSerializer.Serialize(jsonObject);
    }

    #region Helper Methods

    private string ExtractUsernameFromActorUrl(string url) => url.Split('/').Last();
    private string ExtractPostIdFromUrl(string url) => url.Split('/').Last();

    #endregion

    private async Task HandleCreateActivityAsync(JsonElement createActivity, DynamoDbService dbService)
    {
        if (!createActivity.TryGetProperty("object", out var postObject) ||
        !postObject.TryGetProperty("attributedTo", out var authorActorUrlElement))
        {
            _logger.LogWarning("Received 'Create' activity with missing 'object' or 'attributedTo' fields.");
            return;
        }
        
        var authorActorUrl = authorActorUrlElement.GetString();
        if (string.IsNullOrEmpty(authorActorUrl)) return;
        var author = ExtractUsernameFromActorUrl(authorActorUrl);
        if (author == null)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var federationService = scope.ServiceProvider.GetRequiredService<IFederationService>();
                var handle = $"{new Uri(authorActorUrl).Segments.Last()}@{new Uri(authorActorUrl).Host}";
                author = await federationService.DiscoverAndCacheUserAsync(handle);
            }
        }
        
        if (author == null)
        {
            _logger.LogWarning("Could not find or discover author for remote post: {ActorUrl}", authorActorUrl);
            return;
            
        }

        var post = new Post
        {
            PK = $"POST#{ExtractId(postObject)}",
            SK = $"POST#{ExtractId(postObject)}",
            AuthorUsername = ExtractUsername(authorActorUrlElement),
            Content = postObject.GetProperty("content").GetString() ?? "",

        };

        var (followers, nextToken) = await dbService.GetFollowersAsync(post.AuthorUsername);
        var followerUsernames = followers.Select(f => f.FollowerInfo.Username).ToList();

        if (!followerUsernames.Any())
        {
            await dbService.PopulateTimelinesAsync(post, new List<string> { "PUBLIC" });
            _logger.LogInformation("Post by {Author} has no followers to deliver to.", post.AuthorUsername);
            return;
        }

        _logger.LogInformation("Populating timelines for {FollowerCount} followers of {Author}.", followerUsernames.Count, post.AuthorUsername);
        await dbService.PopulateTimelinesAsync(post, followerUsernames);
    }

    private string ExtractId(JsonElement element) =>
        element.GetProperty("id").GetString()?.Split('/').Last() ?? "";

    private string ExtractUsername(JsonElement element) =>
        element.GetString()?.Split('/').Last() ?? "";
}