
using MediatR;
using social_media9.Api.Models;

namespace social_media9.Api.Queries.Posts;

public class PaginatedPostsResponse
    {
        public List<PostResponse> Items { get; set; } = new();
        public string? NextCursor { get; set; }
    }

public record GetPostsByUserQuery(
        string Username,
        int PageSize = 15,
        string? Cursor = null
    ) : IRequest<PaginatedPostsResponse>;