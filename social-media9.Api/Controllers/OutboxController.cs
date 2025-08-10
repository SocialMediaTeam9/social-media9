using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using social_media9.Api.Services.DynamoDB;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

[ApiController]
public class OutboxController : ControllerBase
{
    private readonly DynamoDbService _dbService;
    private readonly IConfiguration _config;

    public OutboxController(DynamoDbService dbService, IConfiguration config)
    {
        _dbService = dbService;
        _config = config;
    }

    [HttpGet("users/{username}/followers")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserFollowers(string username, [FromQuery] bool page = false, [FromQuery] string? cursor = null)
    {
        var user = await _dbService.GetUserProfileByUsernameAsync(username);
        if (user == null) return NotFound();

        var domain = _config["DomainName"];
        var followersUrl = $"https://{domain}/users/{username}/followers";

        if (page)
        {
            var (followers, nextToken) = await _dbService.GetFollowersAsync(username, 15, cursor);

            var pageResponse = new
            {
                @context = _contextUrls,
                id = $"{followersUrl}?page=true" + (string.IsNullOrEmpty(cursor) ? "" : $"&cursor={WebUtility.UrlEncode(cursor)}"),
                type = "OrderedCollectionPage",
                partOf = followersUrl,
                orderedItems = followers.Select(f => (object)f.FollowerInfo.ActorUrl).ToList(),
                next = string.IsNullOrEmpty(nextToken) ? null : $"{followersUrl}?page=true&cursor={WebUtility.UrlEncode(nextToken)}"
            };

            return Content(JsonSerializer.Serialize(pageResponse, _jsonOpts), "application/activity+json");
        }
        else
        {
            var collectionResponse = new
            {
                @context = _contextUrls,
                id = followersUrl,
                type = "OrderedCollection",
                totalItems = user.FollowersCount,
                first = $"{followersUrl}?page=true"
            };

            return Content(JsonSerializer.Serialize(collectionResponse, _jsonOpts), "application/activity+json");
        }
    }

    private static readonly string[] _contextUrls = new[]
   {
        "https://www.w3.org/ns/activitystreams"
    };

    private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    [HttpGet("users/{username}/following")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserFollowing(string username, [FromQuery] bool page = false, [FromQuery] string? cursor = null)
    {
        var user = await _dbService.GetUserProfileByUsernameAsync(username);
        if (user == null) return NotFound();

        var domain = _config["DomainName"];
        var followingUrl = $"https://{domain}/users/{username}/following";

        if (page)
        {
            var (following, nextToken) = await _dbService.GetFollowingAsync(username, 15, cursor);

            var pageResponse = new
            {
                @context = _contextUrls,
                id = $"{followingUrl}?page=true" + (string.IsNullOrEmpty(cursor) ? "" : $"&cursor={WebUtility.UrlEncode(cursor)}"),
                type = "OrderedCollectionPage",
                partOf = followingUrl,
                orderedItems = following.Select(f => (object)f.FollowingInfo.ActorUrl).ToList(),
                next = string.IsNullOrEmpty(nextToken) ? null : $"{followingUrl}?page=true&cursor={WebUtility.UrlEncode(nextToken)}"
            };

            return Content(JsonSerializer.Serialize(pageResponse, _jsonOpts), "application/activity+json");
        }
        else
        {
            var collectionResponse = new
            {
                @context = _contextUrls,
                id = followingUrl,
                type = "OrderedCollection",
                totalItems = user.FollowingCount,
                first = $"{followingUrl}?page=true"
            };

            return Content(JsonSerializer.Serialize(collectionResponse, _jsonOpts), "application/activity+json");
        }
    }


    [HttpGet("/users/{username}/outbox")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOutbox(
    string username,
    [FromQuery] bool page = false,
    [FromQuery] string? cursor = null)
    {
        var user = await _dbService.GetUserProfileByUsernameAsync(username);
        if (user == null) return NotFound();

        var domain = _config["DomainName"];
        var outboxUrl = $"https://{domain}/users/{username}/outbox";

        if (page)
        {
            var (posts, nextToken) = await _dbService.GetPostsByUserAsync(username, 15, cursor);

            var pageResponse = new
            {
                @context = _contextUrls,
                id = $"{outboxUrl}?page=true" + (string.IsNullOrEmpty(cursor) ? "" : $"&cursor={WebUtility.UrlEncode(cursor)}"),
                type = "OrderedCollectionPage",
                partOf = outboxUrl,
                orderedItems = posts.Select(p => JsonSerializer.Deserialize<object>(p.ActivityJson)!).ToList(),
                next = string.IsNullOrEmpty(nextToken) ? null : $"{outboxUrl}?page=true&cursor={WebUtility.UrlEncode(nextToken)}"
            };

            return Content(JsonSerializer.Serialize(pageResponse, _jsonOpts), "application/activity+json");
        }
        else
        {
            var collectionResponse = new
            {
                @context = _contextUrls,
                id = outboxUrl,
                type = "OrderedCollection",
                totalItems = user.PostCount,
                first = $"{outboxUrl}?page=true"
            };

            return Content(JsonSerializer.Serialize(collectionResponse, _jsonOpts), "application/activity+json");
        }
    }
}