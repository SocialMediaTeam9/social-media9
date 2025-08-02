using MediatR;
using social_media9.Api.Data;
using social_media9.Api.Models;
using social_media9.Api.Commands;

public class AddCommentHandler : IRequestHandler<AddCommentCommand, CommentDto>
{
    private readonly ICommentRepository _repository;

    public AddCommentHandler(ICommentRepository repository)
    {
        _repository = repository;
    }

    public async Task<CommentDto> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = new Comment
        {
            CommentId = Guid.NewGuid().ToString(),
            ContentId = request.ContentId,
            UserId = request.UserId,
            Username = request.Username,
            Text = request.Text,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.SaveCommentAsync(comment);
        return new CommentDto(comment);
    }
}
