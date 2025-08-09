using social_media9.Api.Models;
using System.Threading.Tasks;

namespace social_media9.Api.Services.Interfaces
{
    public interface IFederationService
    {
        /// <summary>
        /// Discovers a remote user via WebFinger and fetches their Actor profile.
        /// If the user is found, they are cached in the local database.
        /// </summary>
        /// <param name="userHandle">The full user handle, e.g., "john@anotherserver.com"</param>
        /// <returns>The cached User object from the local database, or null if not found.</returns>
        Task<User?> DiscoverAndCacheUserAsync(string userHandle);

        Task<string?> ResolveInboxUrlAsync(string actorUrl);
    }
}