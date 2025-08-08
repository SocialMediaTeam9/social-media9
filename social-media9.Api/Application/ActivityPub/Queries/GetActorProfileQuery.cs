using MediatR;
using social_media9.Api.Domain.ActivityPub.Entities;
using social_media9.Api.Infrastructure.ActivityPub.Services;
using social_media9.Api.Repositories.Interfaces;

namespace social_media9.Api.Application.ActivityPub.Queries;

public record GetActorProfileQuery(string Username) : IRequest<Actor?>;

public record ResolveWebFingerQuery(string Resource) : IRequest<object?>;

public record GetOutboxActivitiesQuery(string Username) : IRequest<IEnumerable<object>>;

public class GetActorProfileQueryHandler : IRequestHandler<GetActorProfileQuery, Actor?>
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;

    public GetActorProfileQueryHandler(IUserRepository userRepository, IConfiguration config)
    {
        _userRepository = userRepository;
        _config = config;
    }

    public async Task<Actor?> Handle(GetActorProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByUsernameAsync(request.Username);
        if (user == null) return null;

        var domain = _config["ActivityPubSettings:Domain"] ?? "localhost";
        var actorId = $"https://{domain}/users/{user.Username}";

        return new Actor
        {
            Id = actorId,
            PreferredUsername = user.Username,
            Inbox = $"{actorId}/inbox",
            Outbox = $"{actorId}/outbox",
            PublicKey = new PublicKey
            {
                Id = $"{actorId}#main-key",
                Owner = actorId,
                PublicKeyPem = user.PublicKeyPem // stored on user
            }
        };
    }
}

public class GetOutboxActivitiesQueryHandler : IRequestHandler<GetOutboxActivitiesQuery, IEnumerable<object>>
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;

    public GetOutboxActivitiesQueryHandler(IPostRepository postRepository, IUserRepository userRepository, IConfiguration config)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _config = config;
    }

    public async Task<IEnumerable<object>> Handle(GetOutboxActivitiesQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByUsernameAsync(request.Username);
        if (user == null) return Enumerable.Empty<object>();

        var posts = await _postRepository.GetByUserAsync(Guid.Parse(user.UserId));
        var domain = _config["ActivityPubSettings:Domain"] ?? "localhost";
        var actor = $"https://{domain}/users/{user.Username}";

        return posts.Select(post => new
        {
            @context = "https://www.w3.org/ns/activitystreams",
            id = $"https://{domain}/activities/{post.PostId}",
            type = "Create",
            actor = actor,
            published = post.CreatedAt,
            to = new[] { "https://www.w3.org/ns/activitystreams#Public" },
            obj = new
            {
                type = "Note",
                id = $"https://{domain}/notes/{post.PostId}",
                attributedTo = actor,
                content = post.Caption,
                published = post.CreatedAt
            }
        });
    }
}
