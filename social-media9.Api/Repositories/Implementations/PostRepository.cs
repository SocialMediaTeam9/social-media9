using System;

using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using social_media9.Api.Models;

using social_media9.Api.Data;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Dtos;

namespace social_media9.Api.Repositories.Implementations
{
public class PostRepository : IPostRepository
{
    private readonly DynamoDbContext _dbContext;

    public PostRepository(DynamoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Post post)
    {
        await _dbContext.Context.SaveAsync(post);
    }

    public async Task<IEnumerable<Post>> GetAllAsync()
    {
        var conditions = new List<ScanCondition>();
        var posts = await _dbContext.Context.ScanAsync<Post>(conditions).GetRemainingAsync();
        return posts;
    }

    public async Task<Post> GetByIdAsync(Guid postId)
    {
        // TODO: Implement get post by id logic
        throw new NotImplementedException();
    }

    // public async Task<bool> UpdateAsync(Guid postId, PostUpdateRequest request, Guid userId)
    // {
    //     // TODO: Implement update post logic
    //     throw new NotImplementedException();
    // }

    public async Task<IEnumerable<Post>> GetByUserAsync(Guid userId)
    {
        // TODO: Implement get posts by user logic
        throw new NotImplementedException();
    }

    public async Task<bool> LikeAsync(Guid postId, Guid userId)
    {
        // Save Like to DynamoDB
        var like = new Like
        {
            PostId = postId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        await _dbContext.Context.SaveAsync(like);
        return true;
    }

    public async Task<bool> UnlikeAsync(Guid postId, Guid userId)
    {
        // TODO: Implement unlike post logic
        throw new NotImplementedException();
    }

    // public async Task<IEnumerable<UserSummaryDTO>> GetLikesAsync(Guid postId)
    // {
    //     // TODO: Implement get post likes logic
    //     throw new NotImplementedException();
    // }

    public async Task<Guid> AddCommentAsync(Guid postId, AddCommentRequest request, Guid userId, Comment comment)
    {
        // Save comment to DynamoDB
        var dbContext = _dbContext.Context;
        await dbContext.SaveAsync(comment);
        return comment.CommentId;
    }

    public async Task<IEnumerable<CommentDTO>> GetCommentsAsync(Guid postId)
    {
        // TODO: Implement get comments logic
        throw new NotImplementedException();
    }

    public async Task<Guid> AddCommentAsync(Guid postId, AddCommentRequest request, Guid userId)
    {
        // For now, username is not fetched; you may want to fetch it from user service/repo
        
        throw new NotImplementedException();
    }
}
}
