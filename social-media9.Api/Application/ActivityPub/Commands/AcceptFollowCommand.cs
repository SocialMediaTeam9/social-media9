using MediatR;
using Newtonsoft.Json.Linq;
using social_media9.Api.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace social_media9.Api.Application.ActivityPub.Commands;

public record AcceptFollowCommand(string LocalUsername, string RemoteActor, string RemoteInbox, string PrivateKeyPem) : IRequest;

public class AcceptFollowCommandHandler : IRequestHandler<AcceptFollowCommand>
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _config;

    public AcceptFollowCommandHandler(IMediator mediator, IConfiguration config)
    {
        _mediator = mediator;
        _config = config;
    }

    public async Task Handle(AcceptFollowCommand request, CancellationToken cancellationToken)
    {
        var domain = _config["ActivityPubSettings:Domain"] ?? "localhost";
        var actorUrl = $"https://{domain}/users/{request.LocalUsername}";

        var activity = new JObject
        {
            ["@context"] = "https://www.w3.org/ns/activitystreams",
            ["id"] = $"{actorUrl}/activities/accept-{Guid.NewGuid()}",
            ["type"] = "Accept",
            ["actor"] = actorUrl,
            ["object"] = new JObject
            {
                ["type"] = "Follow",
                ["actor"] = request.RemoteActor,
                ["object"] = actorUrl
            }
        };

        await _mediator.Send(new SendActivityCommand(
            request.RemoteInbox,
            activity,
            request.PrivateKeyPem
        ), cancellationToken);
    }
}
