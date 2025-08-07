using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using social_media9.Api.Services.DynamoDB;
using System.Linq;
using System.Text.Json;
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


    [HttpGet("/users/{username}/outbox")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOutbox(
        string username,
        [FromQuery] bool page = false,
        [FromQuery] string? cursor = null)
    {
        var user = await _dbService.GetUserProfileByUsernameAsync(username);
        if (user == null)
        {
            return NotFound($"User '{username}' not found.");
        }

        var domain = _config["DomainName"];
        var outboxUrl = $"https://{domain}/users/{username}/outbox";

        if (page)
        {
            int pageSize = 15;
            var (posts, nextToken) = await _dbService.GetPostsByUserAsync(username, pageSize, cursor);

            var pageResponse = new OrderedCollectionPage
            {
                Id = $"{outboxUrl}?page=true" + (!string.IsNullOrEmpty(cursor) ? $"&cursor={cursor}" : ""),
                PartOf = outboxUrl,
                
                OrderedItems = posts.Select(p => JsonSerializer.Deserialize<object>(p.ActivityJson)!).ToList()
            };

            if (!string.IsNullOrEmpty(nextToken))
            {
                pageResponse.Next = $"{outboxUrl}?page=true&cursor={nextToken}";
            }

            return Ok(pageResponse);
        }

        else
        {
            var collectionResponse = new OrderedCollection
            {
                Id = outboxUrl,
                TotalItems = user.PostCount,
                First = $"{outboxUrl}?page=true"
            };

            return Ok(collectionResponse);
        }
    }

    // /// <summary>
    // /// </summary>
    // [HttpGet("/users/{username}/outbox", Condition = "Request.Query.ContainsKey(\"page\")")]
    // public async Task<IActionResult> GetOutboxPage(string username, [FromQuery] string? cursor = null)
    // {
    //     var domain = _config["DomainName"];
    //     var outboxUrl = $"https://{domain}/users/{username}/outbox";
    //     int pageSize = 10;

    //     var (posts, nextToken) = await _dbService.GetPostsByUserAsync(username, pageSize, cursor);

    //     var page = new OrderedCollectionPage
    //     {
    //         Id = $"{outboxUrl}?page=true" + (cursor != null ? $"&cursor={cursor}" : ""),
    //         PartOf = outboxUrl,
    //         OrderedItems = posts.Select(p => JsonSerializer.Deserialize<object>(p.ActivityJson)!).ToList()
    //     };

    //     if (!string.IsNullOrEmpty(nextToken))
    //     {
    //         page.Next = $"{outboxUrl}?page=true&cursor={nextToken}";
    //     }

    //     return Ok(page);
    // }
}