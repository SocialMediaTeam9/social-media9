using social_media9.Api.Models;
using social_media9.Api.Services.DynamoDB;


public interface ICommentService
{
    Task<Comment?> CreateCommentAsync(string postId, string authorUsername, string content);
}

public class CommentService: ICommentService
{
    private readonly DynamoDbService _dbService;

    public CommentService(DynamoDbService dbService)
    {
        _dbService = dbService;
    }


    public async Task<Comment?> CreateCommentAsync(string postId, string authorUsername, string content)
    {
        var commentId = Ulid.NewUlid().ToString();

        var newComment = new Comment
        {
            PK = $"POST#{postId}",
            SK = $"COMMENT#{commentId}",
            GSI1PK = $"USER#{authorUsername}",
            GSI1SK = $"COMMENT#{commentId}",
            Username = authorUsername,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        var success = await _dbService.CreateCommentAsync(newComment);

        return success ? newComment : null;
    }
}