using MediatR;
using social_media9.Api.Services.DynamoDB;
using System.Threading;
using System.Threading.Tasks;

namespace social_media9.Api.Queries
{
    public class IsFollowingQueryHandler : IRequestHandler<IsFollowingQuery, bool>
    {
        private readonly DynamoDbService _dbService;

        public IsFollowingQueryHandler(DynamoDbService dbService)
        {
            _dbService = dbService;
        }

        public async Task<bool> Handle(IsFollowingQuery request, CancellationToken cancellationToken)
        {
            // The handler's job is to simply call the underlying data service method.
            // All the complex DynamoDB logic is already encapsulated there.
            return await _dbService.IsFollowingAsync(request.LocalUsername, request.TargetUsername);
        }
    }
}