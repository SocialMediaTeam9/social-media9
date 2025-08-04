using MediatR;
using social_media9.Api.Data;
using social_media9.Api.Models;
using social_media9.Api.Dtos;
using social_media9.Api.Commands;
using social_media9.Api.Repositories.Interfaces;

public class AddCommentHandler : IRequestHandler<AddCommentCommand, CommentResponse>
{
    private readonly ICommentRepository _repository;
    private readonly CommentService _commentService;
    private readonly IUserRepository _userRepository;


    public AddCommentHandler(ICommentRepository repository, CommentService commentService, IUserRepository userRepository)
    {
        _repository = repository;
        _commentService = commentService;
        _userRepository = userRepository;
    }

    public async Task<CommentResponse> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {

        var author = await _userRepository.GetUserByIdAsync(request.UserId);
        if (author == null)
        {
            throw new ApplicationException("Author not found.");
        }

        var newCommentEntity = await _commentService.CreateCommentAsync(
            request.PostId,
            author.Username,
            request.Content
        );

        if (newCommentEntity == null)
        {
            throw new ApplicationException("Failed to create comment. The post may not exist.");
        }

        return new CommentResponse(
            CommentId: newCommentEntity.SK.Replace("COMMENT#", ""),
            PostId: newCommentEntity.PK.Replace("POST#", ""),
            AuthorUsername: newCommentEntity.Username,
            Content: newCommentEntity.Content,
            CreatedAt: newCommentEntity.CreatedAt
        );
    }
}
