using social_media9.Api.Application.ActivityPub.Queries;
using MediatR;

namespace social_media9.Api.Infrastructure.ActivityPub.Services;

public class WebFingerService : IRequestHandler<ResolveWebFingerQuery, object?>
{
    private readonly IActorStorageService _storage;
    private readonly IConfiguration _config;

    public WebFingerService(IActorStorageService storage, IConfiguration config)
    {
        _storage = storage;
        _config = config;
    }

    public async Task<object?> Handle(ResolveWebFingerQuery request, CancellationToken cancellationToken)
    {
        var username = request.Resource.Replace("acct:", "").Split('@')[0];
        var domain = _config["ActivityPubSettings:Domain"] ?? "localhost";

        var actor = await _storage.GetActorAsync(username);
        if (actor == null) return null;

        return new
        {
            subject = request.Resource,
            links = new[]
            {
                new {
                    rel = "self",
                    type = "application/activity+json",
                    href = actor.Id
                }
            }
        };
    }
}
