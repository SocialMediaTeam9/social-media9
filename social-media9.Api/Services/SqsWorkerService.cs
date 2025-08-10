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

        using var activityDoc = JsonDocument.Parse(message.Body);
        var activity = activityDoc.RootElement;

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
                string? followTargetUrl = null;
                if (activity.TryGetProperty("object", out var objProp))
                {
                    if (objProp.ValueKind == JsonValueKind.String)
                    {
                        followTargetUrl = objProp.GetString();
                    }
                    else if (objProp.ValueKind == JsonValueKind.Object &&
                             objProp.TryGetProperty("id", out var idProp) &&
                             idProp.ValueKind == JsonValueKind.String)
                    {
                        followTargetUrl = idProp.GetString();
                    }
                }

                if (string.IsNullOrEmpty(followTargetUrl))
                {
                    _logger.LogWarning("Follow activity missing target URL. Actor: {ActorUrl}", actorUrl);
                    break;
                }

                var followedUsername = ExtractUsernameFromActorUrl(followTargetUrl);
                var followerUsername = ExtractUsernameFromActorUrl(actorUrl);

                // 2️⃣ Skip if already following
                if (await dbService.IsFollowingAsync(followerUsername, followedUsername))
                {
                    _logger.LogInformation("Already following {FollowedUsername}, skipping Accept send.", followedUsername);
                    break;
                }

                // 3️⃣ Store follow in DB
                if (!await dbService.ProcessFollowActivityAsync(actorUrl, followedUsername))
                {
                    _logger.LogWarning("Failed to process follow in DB for {Follower} -> {Followed}", followerUsername, followedUsername);
                    break;
                }

                // 4️⃣ Get local followed user entity and validate
                var followedUserEntity = await dbService.GetUserProfileByUsernameAsync(followedUsername);
                if (followedUserEntity == null)
                {
                    _logger.LogError("Cannot send Accept: local user '{FollowedUsername}' not found", followedUsername);
                    break;
                }
                if (string.IsNullOrEmpty(followedUserEntity.ActorUrl))
                {

                    // await dbService.UpdateUserActorUrlAsync(followedUserEntity.Username, followedUserEntity.ActorUrl);
                    _logger.LogError("Cannot send Accept: local user '{FollowedUsername}' has no ActorUrl", followedUsername);
                    // break;
                }
                if (string.IsNullOrEmpty(followedUserEntity.PrivateKeyPem))
                {
                    _logger.LogError("Cannot send Accept: local user '{FollowedUsername}' has no PrivateKeyPem", followedUsername);
                    break;
                }

                var localActorUrl = string.IsNullOrEmpty(followedUserEntity.ActorUrl) ? $"https://{_config["DomainName"]}/users/{followedUsername}" : followedUserEntity.ActorUrl;
                var localPrivateKey = followedUserEntity.PrivateKeyPem;

                // 5️⃣ Resolve remote inbox
                var targetInbox = await ResolveInboxUrlAsync(actorUrl, httpClientFactory);
                if (string.IsNullOrEmpty(targetInbox))
                {
                    _logger.LogWarning("Could not resolve inbox for {ActorUrl}", actorUrl);
                    break;
                }

                // 6️⃣ Build Accept activity
                var acceptActivity = new
                {
                    @context = "https://www.w3.org/ns/activitystreams",
                    id = $"https://{_config["DomainName"]}/activities/{Ulid.NewUlid()}",
                    type = "Accept",
                    actor = localActorUrl,
                    @object = activity,
                    to = new[] { actorUrl }
                };

                var activityDocu = JsonDocument.Parse(JsonSerializer.Serialize(acceptActivity));

                // 7️⃣ Deliver signed Accept
                var httpClient = httpClientFactory.CreateClient("FederationClient");
                var deliveryService = new ActivityPubService(httpClient, localActorUrl, localPrivateKey, _config);
                await deliveryService.DeliverActivityAsync(targetInbox, activityDocu);

                // 8️⃣ OPTIONAL — Push recent posts to new follower
                var (recentPosts, _) = await dbService.GetPostsByUserAsync(followedUsername, 5, null);
                foreach (var post in recentPosts)
                {
                    using var postDoc = JsonDocument.Parse(post.ActivityJson);
                    await deliveryService.DeliverActivityAsync(targetInbox, postDoc);
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

    private static async Task<string?> ResolveInboxFromActorAsync(string actorUrl, IHttpClientFactory httpClientFactory)
    {
        var http = httpClientFactory.CreateClient();
        http.DefaultRequestHeaders.Accept.Clear();
        http.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/activity+json")
        );

        using var resp = await http.GetAsync(actorUrl);
        if (!resp.IsSuccessStatusCode)
            return null;

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        if (doc.RootElement.TryGetProperty("inbox", out var inboxElement))
        {
            return inboxElement.GetString();
        }
        return null;
    }

    private async Task<string?> ResolveInboxUrlAsync(string actorUrl, IHttpClientFactory httpClientFactory)
    {
        var client = httpClientFactory.CreateClient("FederationClient");

        // 1️⃣ If it's an acct:username@domain format, resolve it via WebFinger first
        if (actorUrl.StartsWith("acct:", StringComparison.OrdinalIgnoreCase))
        {
            var acct = actorUrl.Substring(5);
            var parts = acct.Split('@', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return null;

            var webfingerUrl = $"https://{parts[1]}/.well-known/webfinger?resource=acct:{acct}";
            var wfRequest = new HttpRequestMessage(HttpMethod.Get, webfingerUrl);
            wfRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/jrd+json"));

            var wfResponse = await client.SendAsync(wfRequest);
            if (!wfResponse.IsSuccessStatusCode) return null;

            var wfJson = await wfResponse.Content.ReadAsStringAsync();
            using var wfDoc = JsonDocument.Parse(wfJson);

            var link = wfDoc.RootElement
                .GetProperty("links")
                .EnumerateArray()
                .FirstOrDefault(l => l.TryGetProperty("rel", out var rel) &&
                                     rel.GetString() == "self" &&
                                     l.TryGetProperty("type", out var type) &&
                                     type.GetString()?.Contains("activity+json") == true);

            if (link.ValueKind != JsonValueKind.Undefined && link.TryGetProperty("href", out var href))
            {
                actorUrl = href.GetString() ?? actorUrl;
            }
        }

        // 2️⃣ Fetch the actor with correct headers
        var request = new HttpRequestMessage(HttpMethod.Get, actorUrl);
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/activity+json"));
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/ld+json"));

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        var rawJson = await response.Content.ReadAsStringAsync();

        // 3️⃣ Try parsing JSON
        try
        {
            using var actorDoc = JsonDocument.Parse(rawJson);
            if (actorDoc.RootElement.TryGetProperty("inbox", out var inboxProp))
                return inboxProp.GetString();
        }
        catch (JsonException ex)
        {
            // Log and bail
            Console.WriteLine($"[ResolveInboxUrlAsync] Failed to parse JSON from {actorUrl}: {ex.Message}");
            Console.WriteLine($"Raw response:\n{rawJson}");
            return null;
        }

        return null;
    }


    private string BuildAcceptActivity(string actorUrl, JsonElement followActivity, string recipientActorUrl)
    {
        var domain = _config["DomainName"];
        var acceptId = $"https://{domain}/users/{actorUrl.Split('/').Last()}/activities/{Ulid.NewUlid()}";

        var acceptObj = new Dictionary<string, object>
        {
            ["@context"] = "https://www.w3.org/ns/activitystreams",
            ["id"] = acceptId,
            ["type"] = "Accept",
            ["actor"] = actorUrl,
            ["object"] = followActivity,
            ["to"] = new[] { recipientActorUrl }
        };

        return JsonSerializer.Serialize(acceptObj);
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