using Amazon.DynamoDBv2.DataModel;
using social_media9.Api.Models.DynamoDb;
using System;
using System.Collections.Generic;

namespace social_media9.Api.Models
{
    [DynamoDBTable("nexusphere-mvp-main-table")]
    public class User : BaseEntity
    {
        public User() { Type = "UserProfile"; }


        [DynamoDBProperty("UserId")]
        public string UserId { get; set; } = Guid.NewGuid().ToString();

        [DynamoDBProperty("GoogleId")]
        public string? GoogleId { get; set; }

        [DynamoDBProperty("FirstName")]
        public string FirstName { get; set; } = string.Empty;

        [DynamoDBProperty("LastName")]
        public string LastName { get; set; } = string.Empty;

        [DynamoDBProperty("Username")]
        public string Username { get; set; } = string.Empty;

        [DynamoDBProperty("Bio")]
        public string Bio { get; set; } = string.Empty;

        [DynamoDBProperty("FullName")]
        public string FullName { get; set; } = string.Empty;

        [DynamoDBProperty("Email")]
        public string Email { get; set; } = string.Empty;

        [DynamoDBProperty("ProfilePictureUrl")]
        public string ProfilePictureUrl { get; set; } = string.Empty;

        [DynamoDBProperty("ProfilePicture")]
        public string? ProfilePicture { get; set; }

        [DynamoDBProperty("Following")]
        public List<string> Following { get; set; } = new();

        [DynamoDBProperty("Followers")]
        public List<string> Followers { get; set; } = new();

        [DynamoDBProperty("FollowersCount")]
        public int FollowersCount { get; set; } = 0;

        [DynamoDBProperty("FollowingCount")]
        public int FollowingCount { get; set; } = 0;

        [DynamoDBProperty("PublicKeyPem")]
        public string PublicKeyPem { get; set; } = string.Empty;

        [DynamoDBProperty("PrivateKeyPem")]
        public string PrivateKeyPem { get; set; } = string.Empty;

        [DynamoDBProperty("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        [DynamoDBProperty("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [DynamoDBProperty("PostCount")]
        public int PostCount { get; set; } = 0;
        
        [DynamoDBProperty("IsRemote")]
        public bool IsRemote { get; set; } = false;

        [DynamoDBProperty("ActorUrl")]
        public string? ActorUrl { get; set; }

        [DynamoDBProperty("InboxUrl")]
        public string? InboxUrl { get; set; }

        [DynamoDBProperty("FollowersUrl")]
        public string? FollowersUrl { get; set; }

        public static implicit operator string?(User? v)
        {
            throw new NotImplementedException();
        }
    }
}
