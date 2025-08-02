using MediatR;
using social_media9.Api.Data;
using social_media9.Api.Commands;

public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, bool>
{
    private readonly ICommentRepository _repository;

    public UpdateCommentCommandHandler(ICommentRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        return await _repository.UpdateCommentAsync(request.CommentId, request.NewContent);
    }
}
