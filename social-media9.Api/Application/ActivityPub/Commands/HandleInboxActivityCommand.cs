using MediatR;
using Newtonsoft.Json.Linq;
using social_media9.Api.Infrastructure.ActivityPub.Services;
using social_media9.Api.Repositories.Interfaces;

namespace social_media9.Api.Application.ActivityPub.Commands;

public record HandleInboxActivityCommand(string Username, JObject Activity) : IRequest;

public class HandleInboxActivityCommandHandler : IRequestHandler<HandleInboxActivityCommand>
{
    private readonly ILogger<HandleInboxActivityCommandHandler> _logger;
    private readonly IFollowRepository _followRepo;
    private readonly IUserRepository _userRepo;

    public HandleInboxActivityCommandHandler(ILogger<HandleInboxActivityCommandHandler> logger, IFollowRepository followRepo, IUserRepository userRepo)
    {
        _logger = logger;
        _followRepo = followRepo;
        _userRepo = userRepo;
    }

    public async Task Handle(HandleInboxActivityCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received Activity for {Username}: {Activity}", request.Username, request.Activity);

        var type = request.Activity.Value<string>("type");
        var actorUri = request.Activity.Value<string>("actor");
        var objectUri = request.Activity.Value<string>("object");

        if (type == "Follow")
        {
            var targetUser = await _userRepo.GetUserByUsernameAsync(request.Username);
            if (targetUser == null) return;

            var remoteActorId = actorUri?.Split("/").Last();
            if (remoteActorId != null)
            {
                // await _followRepo.AddFollowAsync(remoteActorId, targetUser.UserId);
                _logger.LogInformation("Accepted follow from {Actor} to {LocalUser}", remoteActorId, request.Username);
                // Optionally: Trigger AcceptActivityCommand here
            }
        }

        // TODO: Handle Accept, Like, Announce etc.
    }
}