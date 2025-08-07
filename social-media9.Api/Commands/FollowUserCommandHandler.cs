using MediatR;
using social_media9.Api.Data;
using System; // For ApplicationException
using System.Threading; // For CancellationToken
using System.Threading.Tasks;
using social_media9.Api.Models;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Services.Interfaces;

namespace social_media9.Api.Commands
{
    public class FollowUserCommandHandler : IRequestHandler<FollowUserCommand, Unit>
    {

        private readonly FollowService _followService;

        public FollowUserCommandHandler(FollowService followService)
        {
            _followService = followService;
        }

        public async Task<Unit> Handle(FollowUserCommand request, CancellationToken cancellationToken)
        {
            await _followService.FollowUserAsync(request.FollowerUsername, request.FollowingUsername);

            return Unit.Value;

        }
    }
}