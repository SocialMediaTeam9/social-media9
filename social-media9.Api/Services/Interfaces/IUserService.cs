// using social_media9.Api.Models;
// using social_media9.Api.Services.DynamoDB;

// public interface IUserService
// {
//     Task<(bool Success, User? User)> RegisterNewUserAsync(string username, string displayName, string email, string? profilePictureUrl);
//     Task<User?> UpdateUserProfileAsync(string username, string newDisplayName, string newBio);
// }

// public class UserService : IUserService
// {
//     private readonly DynamoDbService _dbService;
//     private readonly IUserRepository _userRepository;
//     private readonly ICryptoService _cryptoService;

//     public UserService(DynamoDbService dbService, IUserRepository userRepository, ICryptoService cryptoService)
//     {
//         _dbService = dbService;
//         _userRepository = userRepository;
//         _cryptoService = cryptoService;
//     }

//     /// <summary>
//     /// Finds a user by their Google ID, or creates a new one if they don't exist.
//     /// This is the primary entry point for Google-based authentication.
//     /// </summary>
//     public async Task<(bool Success, UserEntity? User)> FindOrCreateUserFromGoogleAsync(string googleId, string email, string displayName, string? profilePictureUrl)
//     {
//         // 1. Check if a user with this Google ID already exists.
//         var existingUser = await _userRepository.GetByGoogleIdAsync(googleId);
//         if (existingUser != null)
//         {
//             // The user already exists, so the operation is successful.
//             return (true, existingUser);
//         }

//         // 2. User does not exist, so we create a new one.
//         // Generate a suggested username, but handle potential collisions.
//         var username = await GenerateUniqueUsernameAsync(email.Split('@')[0]);

//         // 3. Generate cryptographic keys for federation.
//         (string publicKey, string privateKey) = _cryptoService.GenerateRsaKeyPair();

//         // 4. Construct the new UserEntity without any password.
//         var newUser = new UserEntity
//         {
//             UserId = Ulid.NewUlid().ToString(),
//             PK = $"USER#{username}",
//             SK = "METADATA",
//             GSI1PK = $"USER#{username}",
//             GSI1SK = "METADATA",
//             GoogleId = googleId,
//             Username = username,
//             DisplayName = displayName,
//             Email = email,
//             ProfilePictureUrl = profilePictureUrl,
//             PublicKeyPem = publicKey,
//             PrivateKeyPem = privateKey,
//             CreatedAt = DateTime.UtcNow
//         };

//         // 5. Call the transactional write method in DynamoDbService.
//         bool success = await _dbService.CreateUserAsync(newUser);

//         return (success, success ? newUser : null);
//     }

//     // Helper method to ensure usernames are unique
//     private async Task<string> GenerateUniqueUsernameAsync(string baseUsername)
//     {
//         var finalUsername = baseUsername;
//         int counter = 1;
//         while (await _userRepository.GetByUsernameAsync(finalUsername) != null)
//         {
//             finalUsername = $"{baseUsername}{counter}";
//             counter++;
//         }
//         return finalUsername;
//     }
    

//     public async Task<User?> UpdateUserProfileAsync(string username, string newDisplayName, string newBio)
//     {
//         // 1. Get the current user profile from the repository
//         var user = await _userRepository.GetByUsernameAsync(username);
//         if (user == null)
//         {
//             return null; // User not found
//         }

//         // 2. Update the properties
//         user.DisplayName = newDisplayName;
//         user.Bio = newBio;
//         // You could add more updatable fields here

//         // 3. Save the updated entity back to the database
//         await _userRepository.SaveAsync(user);

//         return user;
//     }

// }