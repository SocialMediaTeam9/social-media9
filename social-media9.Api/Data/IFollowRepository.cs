using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using social_media9.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace social_media9.Api.Data
{
    /// <summary>
    /// Defines the contract for a repository that manages user follow relationships.
    /// </summary>
    public interface IFollowRepository
    {
        /// <summary>
        /// Asynchronously checks if one user is following another.
        /// </summary>
        /// <param name="followerId">The ID of the user who is following.</param>
        /// <param name="followingId">The ID of the user being followed.</param>
        /// <returns>A task that returns true if the follower is following the other user, otherwise false.</returns>
        Task<bool> IsFollowingAsync(string followerId, string followingId);

        /// <summary>
        /// Asynchronously adds a new follow relationship.
        /// </summary>
        /// <param name="followerId">The ID of the user who is starting to follow.</param>
        /// <param name="followingId">The ID of the user being followed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddFollowAsync(string followerId, string followingId);

        /// <summary>
        /// Asynchronously removes an existing follow relationship.
        /// </summary>
        /// <param name="followerId">The ID of the user who is unfollowing.</param>
        /// <param name="followingId">The ID of the user being unfollowed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveFollowAsync(string followerId, string followingId);

        /// <summary>
        /// Asynchronously gets a list of user IDs for all followers of a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user whose followers are being retrieved.</param>
        /// <returns>A task that returns an enumerable collection of follower IDs.</returns>
        Task<IEnumerable<string>> GetFollowersAsync(string userId);

        /// <summary>
        /// Asynchronously gets a list of user IDs for all users a specific user is following.
        /// </summary>
        /// <param name="userId">The ID of the user whose following list is being retrieved.</param>
        /// <returns>A task that returns an enumerable collection of user IDs being followed.</returns>
        Task<IEnumerable<string>> GetFollowingAsync(string userId);
    }
}


