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
            return NotFound();
        }

        var domain = _config["DomainName"];
        var outboxUrl = $"https://{domain}/users/{username}/outbox";

        if (page)
        {
            int pageSize = 15;
            var (posts, nextToken) = await _dbService.GetPostsByUserAsync(username, pageSize, cursor);

            var pageResponse = new
            {
                @context = "https://www.w3.org/ns/activitystreams",
                id = $"{outboxUrl}?page=true" + (string.IsNullOrEmpty(cursor) ? "" : $"&cursor={cursor}"),
                type = "OrderedCollectionPage",
                partOf = outboxUrl,
                orderedItems = posts.Select(p => JsonSerializer.Deserialize<object>(p.ActivityJson)!).ToList(),
                next = string.IsNullOrEmpty(nextToken) ? null : $"{outboxUrl}?page=true&cursor={nextToken}"
            };

            return new JsonResult(pageResponse)
            {
                ContentType = "application/activity+json"
            };
        }
        else
        {
            var collectionResponse = new
            {
                @context = "https://www.w3.org/ns/activitystreams",
                id = outboxUrl,
                type = "OrderedCollection",
                totalItems = user.PostCount,
                first = $"{outboxUrl}?page=true"
            };

            return new JsonResult(collectionResponse)
            {
                ContentType = "application/activity+json"
            };
        }
    }
}