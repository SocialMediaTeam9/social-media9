using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using social_media9.Api.Models;
using social_media9.Api.Dtos;

using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Services;
using social_media9.Api.Services.Interfaces;

namespace social_media9.Api.Services.Implementations
{
public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IStorageService _storageService;

    public PostService(IPostRepository postRepository, IStorageService storageService)
    {
        _postRepository = postRepository;
        _storageService = storageService;
    }

    public async Task<Guid> CreatePostAsync(CreatePostRequest request, Guid userId)
    {
        string? mediaUrl = null;

        // Upload media if present
        if (request.MediaFile != null)
        {
            mediaUrl = await _storageService.UploadFileAsync(request.MediaFile);
        }

        var post = new Post
        {
            Id = Guid.NewGuid(),
            Content = request.Content,
            MediaUrl = mediaUrl,
            MediaType = request.MediaType ?? "none",
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _postRepository.AddAsync(post);
        return post.Id;
    }

    public async Task<IEnumerable<PostDTO>> GetPostsAsync()
    {
        var posts = await _postRepository.GetAllAsync();
        return posts.Select(post => new PostDTO
        {
            Id = post.Id,
            Content = post.Content,
            MediaUrl = post.MediaUrl,
            MediaType = post.MediaType,
            UserId = post.UserId,
            CreatedAt = post.CreatedAt
        });
    }
    // Get post by id
    public async Task<PostDTO> GetPostAsync(Guid postId)
    {
        // TODO: Implement get post by id logic
        throw new NotImplementedException();
    }

    // // Get posts by user
    // public async Task<IEnumerable<PostDTO>> GetUserPostsAsync(Guid userId)
    // {
    //     // TODO: Implement get posts by user logic
    //     throw new NotImplementedException();
    // }

    // Like post
    public async Task<bool> LikePostAsync(Guid postId, Guid userId)
    {
        // Check if already liked (optional, for idempotency)
        return await _postRepository.LikeAsync(postId, userId);
    }

    // // Get post likes
    // public async Task<IEnumerable<UserSummaryDTO>> GetPostLikesAsync(Guid postId)
    // {
    //     // TODO: Implement get post likes logic
    //     throw new NotImplementedException();
    // }

    // Add comment
    public async Task<Guid> AddCommentAsync(Guid postId, AddCommentRequest request, Guid userId)
    {
        // For now, username is not fetched; you may want to fetch it from user service/repo
        var comment = new Comment
        {
            CommentId = Guid.NewGuid(),
            PostId = postId,
            UserId = userId,
            Username = "", // TODO: fetch username from user service if needed
            Text = request.Text,
            CreatedAt = DateTime.UtcNow
        };
        await _postRepository.AddCommentAsync(postId, request, userId, comment);
        return comment.CommentId;
    }

    // Get comments
    public async Task<IEnumerable<CommentDTO>> GetCommentsAsync(Guid postId)
    {
        // TODO: Implement get comments logic
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<PostDTO>> GetUserPostsAsync(Guid userId) 
    {
        throw new NotImplementedException();
    }
}
}