using MediatR;
using social_media9.Api.Data;
using social_media9.Api.Models;
using social_media9.Api.Dtos;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Services.DynamoDB;
using social_media9.Api.Services;


public class GetCommentsByContentHandler : IRequestHandler<GetCommentsByContentQuery, List<CommentResponse>>
{
    private readonly DynamoDbService _dbService;

    public GetCommentsByContentHandler(DynamoDbService dbService)
    {
        _dbService = dbService;
    }

    public async Task<List<CommentResponse>> Handle(GetCommentsByContentQuery request, CancellationToken cancellationToken)
    {
        var commentEntities = await _dbService.GetCommentsForPostAsync(request.PostId);

        var response = commentEntities.Select(entity => new CommentResponse(
            CommentId: entity.SK.Replace("COMMENT#", ""),
            PostId: entity.PK.Replace("POST#", ""),
            AuthorUsername: entity.Username,
            Content: entity.Content,
            CreatedAt: entity.CreatedAt
        )).ToList();

        return response;

        // var comments = await _repository.GetCommentsByContentAsync(request.PostId);
        // return comments.Select(c => new CommentDto(c)).ToList();
    }
}
