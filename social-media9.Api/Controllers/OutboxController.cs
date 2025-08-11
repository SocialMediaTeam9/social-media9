using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using social_media9.Api.Services.DynamoDB;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using social_media9.Api.Models.ActivityPub;

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
        ForceActivityJson();

        var user = await _dbService.GetUserProfileByUsernameAsync(username);
        if (user == null) return NotFound();

        var domain = _config["DomainName"];
        var followersUrl = $"https://{domain}/users/{username}/followers";

        if (page)
        {
            var (followers, nextToken) = await _dbService.GetFollowersAsync(username, 15, cursor);

            if (followers.Count == 0)
                return NotFound();

            var pageResponse = new ActivityPubCollectionPage
            {
                Context = _contextUrls,
                Id = $"{followersUrl}?page=true" + (string.IsNullOrEmpty(cursor) ? "" : $"&cursor={WebUtility.UrlEncode(cursor)}"),
                Type = "OrderedCollectionPage",
                PartOf = followersUrl,
                OrderedItems = followers.Select(f => (object)f.FollowerInfo.ActorUrl).ToList(),
                Next = string.IsNullOrEmpty(nextToken) ? null : $"{followersUrl}?page=true&cursor={WebUtility.UrlEncode(nextToken)}"
            };

            return Content(JsonSerializer.Serialize(pageResponse, _jsonOpts), "application/activity+json");
        }
        else
        {
            var collectionResponse = new ActivityPubCollection
            {
                Context = new[] { "https://www.w3.org/ns/activitystreams" },
                Id = followersUrl,
                Type = "OrderedCollection",
                TotalItems = user.FollowersCount,
                First = $"{followersUrl}?page=true"
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
        PropertyNamingPolicy = null,
        WriteIndented = false
    };

    [HttpGet("users/{username}/following")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserFollowing(string username, [FromQuery] bool page = false, [FromQuery] string? cursor = null)
    {

        ForceActivityJson();

        var user = await _dbService.GetUserProfileByUsernameAsync(username);
        if (user == null) return NotFound();

        var domain = _config["DomainName"];
        var followingUrl = $"https://{domain}/users/{username}/following";

        if (page)
        {
            var (following, nextToken) = await _dbService.GetFollowingAsync(username, 15, cursor);

            if (following.Count == 0)
                return NotFound();

            var pageResponse = new ActivityPubCollectionPage
            {
                Context = _contextUrls,
                Id = $"{followingUrl}?page=true" + (string.IsNullOrEmpty(cursor) ? "" : $"&cursor={WebUtility.UrlEncode(cursor)}"),
                Type = "OrderedCollectionPage",
                PartOf = followingUrl,
                OrderedItems = following.Select(f => (object)f.FollowingInfo.ActorUrl).ToList(),
                Next = string.IsNullOrEmpty(nextToken) ? null : $"{followingUrl}?page=true&cursor={WebUtility.UrlEncode(nextToken)}"
            };

            return Content(JsonSerializer.Serialize(pageResponse, _jsonOpts), "application/activity+json");
        }
        else
        {
            var collectionResponse = new
            {
                Context = _contextUrls,
                Id = followingUrl,
                Type = "OrderedCollection",
                TotalItems = user.FollowingCount,
                First = $"{followingUrl}?page=true"
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
        ForceActivityJson();

        var user = await _dbService.GetUserProfileByUsernameAsync(username);
        if (user == null) return NotFound();

        var domain = _config["DomainName"];
        var outboxUrl = $"https://{domain}/users/{username}/outbox";

        if (page)
        {
            var (posts, nextToken) = await _dbService.GetPostsByUserAsync(username, 15, cursor);

            if (posts.Count == 0)
                return NotFound();

            var pageResponse = new ActivityPubCollectionPage
            {
                Context = _contextUrls,
                Id = $"{outboxUrl}?page=true" + (string.IsNullOrEmpty(cursor) ? "" : $"&cursor={WebUtility.UrlEncode(cursor)}"),
                Type = "OrderedCollectionPage",
                PartOf = outboxUrl,
                OrderedItems = posts.Select(p => JsonSerializer.Deserialize<object>(p.ActivityJson)!).ToList(),
                Next = string.IsNullOrEmpty(nextToken) ? null : $"{outboxUrl}?page=true&cursor={WebUtility.UrlEncode(nextToken)}"
            };

            return Content(JsonSerializer.Serialize(pageResponse, _jsonOpts), "application/activity+json");
        }
        else
        {
            var (firstPosts, firstNextToken) = await _dbService.GetPostsByUserAsync(username, 15, null);

            var outboxWithItems = new
            {
                @context = _contextUrls,
                id = outboxUrl,
                type = "OrderedCollection",
                totalItems = user.PostCount,
                first = new
                {
                    id = $"{outboxUrl}?page=true",
                    type = "OrderedCollectionPage",
                    partOf = outboxUrl,
                    orderedItems = firstPosts.Select(p => JsonSerializer.Deserialize<object>(p.ActivityJson)!).ToList(),
                    next = string.IsNullOrEmpty(firstNextToken) ? null : $"{outboxUrl}?page=true&cursor={WebUtility.UrlEncode(firstNextToken)}"
                }
            };

            return Content(JsonSerializer.Serialize(outboxWithItems, _jsonOpts), "application/activity+json");
        }
    }

    private void ForceActivityJson()
    {
        if (Request.Headers.Accept.Any(a => a.Contains("application/activity+json")))
        {
            Response.ContentType = "application/activity+json";
        }
    }
}