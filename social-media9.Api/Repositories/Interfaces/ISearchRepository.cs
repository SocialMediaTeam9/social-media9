using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using social_media9.Api.Models;

namespace social_media9.Api.Repositories.Interfaces
{
    public interface ISearchRepository
    {
        Task<IEnumerable<PostSearchDocument>> SearchPostsAsync(string query, int limit, CancellationToken cancellationToken);
        Task<IEnumerable<UserSearchDocument>> SearchUsersAsync(string query, int limit, CancellationToken cancellationToken);
        Task<IEnumerable<PostSearchDocument>> SearchHashtagsAsync(string tag, int limit, CancellationToken cancellationToken);
    }
}