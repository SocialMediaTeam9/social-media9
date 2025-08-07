using MediatR;
using social_media9.Api.Data;
using social_media9.Api.Commands;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Services;

public class DeleteCommentHandler : IRequestHandler<DeleteCommentCommand, bool>
{
    private readonly ICommentRepository _repository;

    public DeleteCommentHandler(ICommentRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        await _repository.DeleteCommentAsync(request.CommentId, request.PostId);
        return true;
    }
}
