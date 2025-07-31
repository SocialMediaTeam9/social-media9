public interface IPostRepository
{
    Task AddAsync(Post post);
    Task<IEnumerable<Post>> GetAllAsync();
}

public class PostRepository : IPostRepository
{
    private readonly AppDbContext _context;
    public PostRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(Post post)
    {
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Post>> GetAllAsync()
    {
        return await Task.FromResult(_context.Posts.ToList());
    }
}