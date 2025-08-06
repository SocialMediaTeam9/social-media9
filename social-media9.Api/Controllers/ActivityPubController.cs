using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using social_media9.Api.Services.DynamoDB;

[ApiController]
public class ActivityPubController : ControllerBase
{
    private readonly DynamoDbService _dbService;
    private readonly IConfiguration _config;
    private readonly string _domainName;
    private readonly string _federationDomain;

    public ActivityPubController(DynamoDbService dbService, IConfiguration config)
    {
        _dbService = dbService;
        _config = config;
        _domainName = _config["DomainName"] ?? throw new InvalidOperationException("DomainName is not configured.");
        _federationDomain = _config["FederationDomain"] ?? throw new InvalidOperationException("FederationDomain is not configured.");
    }

    // Handles: https://peerspace.online/.well-known/webfinger
    [HttpGet("/.well-known/webfinger")]
    public async Task<IActionResult> WebFinger([FromQuery] string resource)
    {
         if (string.IsNullOrEmpty(resource) || !resource.StartsWith("acct:"))
        {
            return BadRequest("Invalid resource format.");
        }

        var parts = resource.Replace("acct:", "").Split('@');
        if (parts.Length != 2) return BadRequest("Invalid resource format.");
        
        var username = parts[0];
        var domain = parts[1];

        // Check if the request is for a user on our domain.
        if (!string.Equals(domain, _domainName, StringComparison.OrdinalIgnoreCase))
        {
            return NotFound($"User domain '{domain}' is not managed by this server.");
        }

        // Check if the user actually exists in our database.
        var user = await _dbService.GetUserProfileByUsernameAsync(username);
        if (user == null)
        {
            return NotFound($"User '{username}' not found.");
        }

        var actorUrl = $"https://{_domainName}/users/{username}";

        var response = new WebFingerResponse(
            Subject: $"acct:{username}@{_domainName}",
            Aliases: new List<string> { actorUrl },
            Links: new List<WebFingerLink>
            {
                new("self", "application/activity+json", actorUrl)
            }
        );

        // JRD (JSON Resource Descriptor) is the correct content type for WebFinger.
        return Content(JsonSerializer.Serialize(response), "application/jrd+json");
    }

    // Handles: https://peerspace.online/users/alice
    [HttpGet("/users/{username}")]
    public async Task<IActionResult> GetActor(string username)
    {
        var user = await _dbService.GetUserProfileByUsernameAsync(username);
        if (user == null)
        {
            return NotFound($"User '{username}' not found.");
        }

        var actorUrl = $"https://{_domainName}/users/{username}";
        var fedActorUrlBase = $"https://{_federationDomain}/users/{username}";
        
        var actorResponse = new ActorResponse(
            Context: new List<string> { "https://www.w3.org/ns/activitystreams", "https://w3id.org/security/v1" },
            Id: actorUrl,
            Type: "Person",
            PreferredUsername: user.Username,
            Name: user.FullName,
            Inbox: $"{actorUrl}/inbox",
            Outbox: $"{fedActorUrlBase}/outbox",
            Followers: $"{actorUrl}/followers",
            Following: $"{actorUrl}/following",

            PublicKey: new ActorPublicKey(
                Id: $"{actorUrl}#main-key",
                Owner: actorUrl,
                PublicKeyPem: user.PublicKeyPem
            )
        );

        return Content(JsonSerializer.Serialize(actorResponse), "application/activity+json");
    }
}