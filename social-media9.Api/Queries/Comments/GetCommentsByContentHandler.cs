using MediatR;
using social_media9.Api.Data;
using social_media9.Api.Models;
using social_media9.Api.Dtos;
using social_media9.Api.Repositories.Interfaces;


public class GetCommentsByContentHandler : IRequestHandler<GetCommentsByContentQuery, List<CommentDto>>
{
    private readonly ICommentRepository _repository;

    public GetCommentsByContentHandler(ICommentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CommentDto>> Handle(GetCommentsByContentQuery request, CancellationToken cancellationToken)
    {
        var comments = await _repository.GetCommentsByContentAsync(request.PostId);
        return comments.Select(c => new CommentDto(c)).ToList();
    }
}
