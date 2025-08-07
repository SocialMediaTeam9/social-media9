using social_media9.Api.Domain.ActivityPub.Entities;
using social_media9.Api.Repositories.Interfaces;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;

namespace social_media9.Api.Infrastructure.ActivityPub.Services;

public class DynamoDbActorStorageService : IActorStorageService
{
    private readonly IUserRepository _userRepo;
    private readonly IPostRepository _postRepo;
    private readonly ILogger<DynamoDbActorStorageService> _logger;

    public DynamoDbActorStorageService(IUserRepository userRepo, IPostRepository postRepo, ILogger<DynamoDbActorStorageService> logger)
    {
        _userRepo = userRepo;
        _postRepo = postRepo;
        _logger = logger;
    }

    public async Task<Actor?> GetActorAsync(string username)
    {
        var user = await _userRepo.GetUserByUsernameAsync(username);
        if (user == null) return null;

        return new Actor
        {
            Id = $"https://yourdomain.com/users/{username}",
            PreferredUsername = username,
            Name = user.FullName,
            Summary = user.Bio,
            Inbox = $"https://yourdomain.com/users/{username}/inbox",
            Outbox = $"https://yourdomain.com/users/{username}/outbox",
            PublicKey = new PublicKey
            {
                Id = $"https://yourdomain.com/users/{username}#main-key",
                Owner = $"https://yourdomain.com/users/{username}",
                Pem = user.PublicKeyPem
            },
            Type = "Person"
        };
    }

    public async Task<IEnumerable<object>> GetOutboxAsync(string username)
    {
        var user = await _userRepo.GetUserByUsernameAsync(username);
        if (user == null) return Enumerable.Empty<object>();

        var posts = await _postRepo.GetByUserAsync(Guid.Parse(user.UserId));
        var activities = posts.Select(post => new
        {
            @context = "https://www.w3.org/ns/activitystreams",
            type = "Create",
            actor = $"https://yourdomain.com/users/{username}",
            to = new[] { "https://www.w3.org/ns/activitystreams#Public" },
            cc = new[] { $"https://yourdomain.com/users/{username}/followers" },
            published = post.CreatedAt,
            object_ = new
            {
                type = "Note",
                id = $"https://yourdomain.com/posts/{post.PostId}",
                content = post.Content,
                attributedTo = $"https://yourdomain.com/users/{username}",
                published = post.CreatedAt,
            }
        });

        return activities.Cast<object>();
    }

    public Task SaveActorAsync(Actor actor)
    {
        _logger.LogInformation("SaveActorAsync called with actor ID: {Id}", actor.Id);
        return Task.CompletedTask;
    }
}