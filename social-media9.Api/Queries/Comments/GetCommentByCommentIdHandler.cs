using MediatR;
using social_media9.Api.Data;
using social_media9.Api.Models;
using social_media9.Api.Dtos;
using social_media9.Api.Repositories.Interfaces;


public class GetCommentByCommentIdHandler : IRequestHandler<GetCommentByCommentIdQuery, CommentDto>
{
    private readonly ICommentRepository _repository;

    public GetCommentByCommentIdHandler(ICommentRepository repository)
    {
        _repository = repository;
    }

    public async Task<CommentDto> Handle(GetCommentByCommentIdQuery request, CancellationToken cancellationToken)
    {
        var comment = await _repository.GetCommentByIdAsync(request.CommentId);
        if (comment == null)
        {
            return null; // or throw an exception if preferred
        }
        return new CommentDto(comment);
    }
}
