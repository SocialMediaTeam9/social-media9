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

        if (request.MediaFile != null)
        {
            mediaUrl = await _storageService.UploadFileAsync(request.MediaFile);
        }

        var post = new Post
        {
            Content = request.Content,
            MediaUrl = mediaUrl,
            MediaType = request.MediaType,
            UserId = userId
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
}