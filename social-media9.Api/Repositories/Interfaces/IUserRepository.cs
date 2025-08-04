using social_media9.Api;
using social_media9.Api.Data;
using social_media9.Api.Models; 
using System.Collections.Generic;
using social_media9.Api.Services;

namespace social_media9.Api.Repositories.Interfaces
{
    public interface IUserRepository
    {
        // Renamed from GetByIdAsync for clarity
        Task<User?> GetUserByIdAsync(string id);

        // Added to support Google login functionality
        Task<User?> GetUserByGoogleIdAsync(string googleId);

        // Added to check if a user exists without fetching the full object
        Task<bool> ExistsAsync(string id);

        // Added method for adding a new user
        Task<User> AddUserAsync(User user);
        
        // Added method for updating an existing user
        Task<User> UpdateUserAsync(User user);

        // Added method for deleting a user
        Task DeleteUserAsync(string id);

        // Added to efficiently get a list of users by their IDs
        Task<IEnumerable<User>> GetUsersByIdsAsync(IEnumerable<string> ids);
    }
}
