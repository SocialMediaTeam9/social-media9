using Amazon.SQS;
using Amazon.SQS.Model;
using social_media9.Api.Models;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Services.DynamoDB;
using social_media9.Api.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;

public class FollowService
{
    private readonly HttpClient _httpClient;
    private readonly IFollowRepository _followRepository;
    private readonly DynamoDbService _dbService;
    private readonly IAmazonSQS _sqsClient;
    private readonly IConfiguration _config;
    private readonly IFederationService _federationService;

    private readonly IHttpClientFactory _httpClientFactory;

    public FollowService(
        DynamoDbService dbService,
        IHttpClientFactory httpClientFactory,
        IFollowRepository followRepository,
        IAmazonSQS sqsClient,
        IConfiguration config,
        IFederationService federationService,
        IHttpClientFactory httpClient
        )
    {
        _dbService = dbService;
        _httpClient = httpClientFactory.CreateClient("FederationClient");
        _followRepository = followRepository;
        _sqsClient = sqsClient;
        _config = config;
        _federationService = federationService;
        _httpClientFactory = httpClient;

    }


    public async Task<bool> FollowUserAsync(string localFollowerUsername, string targetHandle)
    {
        var localFollower = await _dbService.GetUserProfileByUsernameAsync(localFollowerUsername);
        if (localFollower == null)
        {
            throw new ApplicationException("Current user not found.");
        }

        var targetUser = await _dbService.GetUserProfileByUsernameAsync(localFollowerUsername);

        if (targetUser == null) {
            targetUser = await _federationService.DiscoverAndCacheUserAsync(targetHandle);
        }

        if (targetUser == null)
        {
            throw new ApplicationException($"User '{targetHandle}' could not be found.");
        }
        
        if (localFollower.UserId == targetUser.UserId)
        {
            throw new ApplicationException("You cannot follow yourself.");
        }

        var localFollowerSummary = new UserSummary("",localFollower.Username, localFollower.ActorUrl, localFollower.ProfilePictureUrl);
        var targetUserSummary = new UserSummary("", targetUser.Username, targetUser.ActorUrl, targetUser.ProfilePictureUrl);
        
        var dbSuccess = await _dbService.ProcessLocalUserFollowAsync(localFollowerSummary, targetUserSummary);

        if (!dbSuccess)
        {
            throw new ApplicationException("You are already following this user.");
        }

       
        if (targetUser.IsRemote)
        {
            string followActivityJson = BuildFollowActivityJson(localFollower.ActorUrl, targetUser.ActorUrl);
            var activityDoc = JsonDocument.Parse(followActivityJson);
            var httpClient = _httpClientFactory.CreateClient("FederationClient");

            var deliveryService = new ActivityPubService(httpClient, localFollower.ActorUrl, localFollower.PrivateKeyPem);

           await deliveryService.DeliverActivityAsync(targetUser.InboxUrl, activityDoc);
        }

        return true;
    }

    private string BuildFollowActivityJson(string followerActorUrl, string followingActorUrl)
    {
        var activity = new
        {
            type = "Follow",
            actor = followerActorUrl,
            to = new[] { followingActorUrl },
            @object = followingActorUrl
        };

        var jsonObject = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(activity));
        jsonObject["@context"] = "https://www.w3.org/ns/activitystreams";
        jsonObject["id"] = $"{followerActorUrl}/follows/{Ulid.NewUlid()}";

        return JsonSerializer.Serialize(jsonObject);
    }

    public async Task<bool> UnfollowUserAsync(string localUsername, string unfollowedActorUrl)
    {


        var localUser = await _dbService.GetUserSummaryAsync(localUsername);

        var unfollowedUsername = unfollowedActorUrl.Split('/').Last();
        var unfollowedUser = new UserSummary("", unfollowedUsername, unfollowedActorUrl, null);

        if (localUser == null || unfollowedUser == null)
        {
            throw new ApplicationException("User not found.");
        }

        string activityJson = BuildUndoFollowActivityJson(localUser.ActorUrl, unfollowedUser.ActorUrl);

        await _sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = _config["Aws:OutboundSqsQueueUrl"],
            MessageBody = activityJson
        });

        return await _dbService.DeleteFollowAndDecrementCountsAsync(localUser, unfollowedUser);
    }

    private string BuildUndoFollowActivityJson(string followerActorUrl, string followingActorUrl)
    {
        var domain = _config["DomainName"];
        var activityId = $"https://{domain}/activities/{Ulid.NewUlid()}";

        var activity = new
        {
            type = "Undo",
            actor = followerActorUrl,
            to = new[] { followingActorUrl },
            @object = new
            {
                type = "Follow",
                actor = followerActorUrl,
                @object = followingActorUrl
            }
        };

        var jsonObject = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(activity));
        jsonObject["@context"] = "https://www.w3.org/ns/activitystreams";
        jsonObject["id"] = activityId; // Add the ID to the top level

        return JsonSerializer.Serialize(jsonObject);
    }

    public async Task<List<UserSummary>> GetFollowersListAsync(string username)
    {

        var followEntities = await _followRepository.GetFollowersAsync(username);
        return followEntities.Select(f => f.FollowerInfo).ToList();

        // var followEntities = await _dbService.GetFollowersAsync(username);
        // return followEntities.Select(f => f.FollowerInfo).ToList();
    }

    public async Task<List<UserSummary>> GetFollowingListAsync(string username)
    {
        // var followEntities = await _dbService.GetFollowingAsync(username);
        // return followEntities.Select(f => f.FollowingInfo).ToList();

        var followEntities = await _followRepository.GetFollowingAsync(username);
        return followEntities.Select(f => f.FollowingInfo).ToList();
    }

    private async Task<UserSummary?> GetRemoteUserSummaryAsync(string actorUrl)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, actorUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/activity+json"));

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var jsonDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

            var username = jsonDoc.RootElement.GetProperty("preferredUsername").GetString();
            var profilePic = jsonDoc.RootElement.TryGetProperty("icon", out var icon)
                ? icon.GetProperty("url").GetString()
                : null;

            if (string.IsNullOrEmpty(username)) return null;

            return new UserSummary("", username, actorUrl, profilePic);
        }
        catch (Exception ex)
        {
            // Console.WriteLine($"Failed to fetch remote actor {actorUrl}. Exception: {ex.Message}");
            return null;
        }
    }
}