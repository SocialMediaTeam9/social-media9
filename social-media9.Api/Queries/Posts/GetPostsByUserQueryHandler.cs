using MediatR;
using social_media9.Api.Models;
using social_media9.Api.Queries.Posts;
using social_media9.Api.Services.DynamoDB;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Peerspace.Api.Handlers
{
    public class GetPostsByUserQueryHandler : IRequestHandler<GetPostsByUserQuery, PaginatedPostsResponse>
    {
        private readonly DynamoDbService _dbService;

        public GetPostsByUserQueryHandler(DynamoDbService dbService)
        {
            _dbService = dbService;
        }

        public async Task<PaginatedPostsResponse> Handle(GetPostsByUserQuery request, CancellationToken cancellationToken)
        {
            var (postEntities, nextToken) = await _dbService.GetPostsByUserAsync(
                request.Username,
                request.PageSize,
                request.Cursor
            );

            var postResponses = postEntities.Select(entity => new PostResponse(
                PostId: entity.SK.Replace("POST#", ""),
                AuthorUsername: entity.AuthorUsername,
                Content: entity.Content,
                CreatedAt: entity.CreatedAt,
                CommentCount: entity.CommentCount,
                Attachments: entity.Attachments
            )).ToList();

            return new PaginatedPostsResponse
            {
                Items = postResponses,
                NextCursor = nextToken
            };
        }
    }
}