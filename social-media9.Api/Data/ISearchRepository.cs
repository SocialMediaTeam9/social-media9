using MediatR;
using social_media9.Api.Models;

namespace social_media9.Api.Data;

public interface ISearchRepository
{
  Task<IEnumerable<VideoPostSummary>> SearchContentAsync(string query, int limit, CancellationToken cancellationToken);
  Task<IEnumerable<UserSummary>> SearchUsersAsync(string query, int limit, CancellationToken cancellationToken);
  Task<IEnumerable<VideoPostSummary>> SearchHashtagsAsync(string tag, int limit, CancellationToken cancellationToken);
  Task<IEnumerable<Notification>> GetNotificationsAsync(string userId, int limit, CancellationToken cancellationToken);
}