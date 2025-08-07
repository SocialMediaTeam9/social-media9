using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using social_media9.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace social_media9.Api.Repositories.Interfaces
{
    /// <summary>
    /// Defines the contract for a repository that manages user follow relationships.
    /// </summary>
    public interface IFollowRepository
    {

        /// <summary>
        /// Asynchronously gets a list of user IDs for all followers of a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user whose followers are being retrieved.</param>
        /// <returns>A task that returns an enumerable collection of follower IDs.</returns>
        Task<IEnumerable<Follow>> GetFollowersAsync(string userId);

        /// <summary>
        /// Asynchronously gets a list of user IDs for all users a specific user is following.
        /// </summary>
        /// <param name="userId">The ID of the user whose following list is being retrieved.</param>
        /// <returns>A task that returns an enumerable collection of user IDs being followed.</returns>
        Task<IEnumerable<Follow>> GetFollowingAsync(string userId);

        Task<bool> IsFollowingAsync(string followerId, string followingId);

    }
}


