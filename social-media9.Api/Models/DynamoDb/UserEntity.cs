// using Amazon.DynamoDBv2.DataModel;
// using social_media9.Api.Models.DynamoDb;


// [DynamoDBTable("nexusphere-mvp-main-table")]
// public class UserEntity : BaseEntity
// {
//     public UserEntity() { Type = "UserProfile"; }

//     [DynamoDBProperty("UserId")]
//     public string UserId { get; set; } = string.Empty;

//     [DynamoDBProperty("Username")]
//     public string Username { get; set; } = string.Empty;

//     [DynamoDBProperty("Email")]
//     public string Email { get; set; } = string.Empty;

//     [DynamoDBProperty("HashedPassword")]
//     public string HashedPassword { get; set; } = string.Empty;

//     [DynamoDBProperty("DisplayName")]
//     public string DisplayName { get; set; } = string.Empty;

//     [DynamoDBProperty("Bio")]
//     public string? Bio { get; set; }

//     [DynamoDBProperty("ProfilePictureUrl")]
//     public string? ProfilePictureUrl { get; set; }

//     [DynamoDBProperty("FollowersCount")]
//     public int FollowersCount { get; set; } = 0;

//     [DynamoDBProperty("FollowingCount")]
//     public int FollowingCount { get; set; } = 0;

//     [DynamoDBProperty("PublicKeyPem")]
//     public string PublicKeyPem { get; set; } = string.Empty;

//     [DynamoDBProperty("PrivateKeyPem")]
//     public string PrivateKeyPem { get; set; } = string.Empty;

//     [DynamoDBProperty("CreatedAt")]
//     public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
// }