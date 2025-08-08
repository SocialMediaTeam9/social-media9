using social_media9.Api;
using social_media9.Api.Data;
using social_media9.Api.Models; 
using System.Collections.Generic;
using social_media9.Api.Services;

namespace social_media9.Api.Repositories.Interfaces
{
    public interface IUserRepository
    {
        
        Task<User?> GetUserByUsernameAsync(string username);

        Task<User?> GetUserByIdAsync(string id);

        
        Task<User?> GetUserByGoogleIdAsync(string googleId);

       
        Task<bool> ExistsAsync(string id);

        
        Task<User> AddUserAsync(User user);

        Task<User> UpdateUserAsync(User user);
        Task DeleteUserAsync(string id);

        Task<IEnumerable<User>> GetUsersByIdsAsync(IEnumerable<string> ids);

        Task<UserSummary?> GetUserSummaryAsync(string username);

        Task<IEnumerable<User>> SearchUsersAsync(string query, int limit);
        
        Task<User?> GetUserByActorUrl(string actorUrl);
    }
}
