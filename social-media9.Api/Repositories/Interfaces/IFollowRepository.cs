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
        Task FollowAsync(string followerId, string followingId);
        Task<bool> UnfollowAsync(string followerId, string followingId);
        Task<IEnumerable<Follow>> GetFollowersAsync(string userId);
        Task<IEnumerable<Follow>> GetFollowingAsync(string userId);
        Task<bool> IsFollowingAsync(string followerId, string followingId);
    }

}


