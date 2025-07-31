public interface IPostService
{
    Task<Guid> CreatePostAsync(CreatePostRequest request, Guid userId);
    Task<IEnumerable<PostDTO>> GetPostsAsync();
}