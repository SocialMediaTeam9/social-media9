using MediatR;

public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, bool>
{
    private readonly ICommentRepository _repository;

    public UpdateCommentCommandHandler(ICommentRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        return await _repository.UpdateCommentAsync(request.ContentId, request.CommentId, request.NewContent);
    }
}
