using Amazon.SQS;
using Amazon.SQS.Model;
using social_media9.Api.Models;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Services.DynamoDB;
using System.Net.Http.Headers;
using System.Text.Json;

public class FollowService
{
    private readonly HttpClient _httpClient;
    private readonly IFollowRepository _followRepository;
    private readonly DynamoDbService _dbService;
    private readonly IAmazonSQS _sqsClient;
    private readonly IConfiguration _config;

    public FollowService(
        DynamoDbService dbService,
        IHttpClientFactory httpClientFactory,
        IFollowRepository followRepository,
        IAmazonSQS sqsClient,
        IConfiguration config)
    {
        _dbService = dbService;
        _httpClient = httpClientFactory.CreateClient("FederationClient");
        _followRepository = followRepository;
        _sqsClient = sqsClient;
        _config = config;
    }


    public async Task<bool> FollowUserAsync(string localUsername, string remoteActorUrl)
    {
        var localUser = await _dbService.GetUserSummaryAsync(localUsername);
        if (localUser == null) return false;


        var remoteUser = await GetRemoteUserSummaryAsync(remoteActorUrl);
        if (remoteUser is null) return false;

        return await _dbService.CreateFollowAndIncrementCountsAsync(localUser, remoteUser);
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
        var activityId = $"https://fed.{domain}/activities/{Ulid.NewUlid()}";

        var activity = new
        {
            type = "Undo",
            actor = followerActorUrl,
            to = new[] { followingActorUrl }, // Address it directly to the person being unfollowed
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