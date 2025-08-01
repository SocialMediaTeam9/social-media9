using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;

namespace social_media9.Api.Models
{
    [DynamoDBTable("social_media9_Users")]
    public class User
    {
        [DynamoDBHashKey]
        public string UserId { get; set; } = Guid.NewGuid().ToString();
        public string? GoogleId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public List<string> Following { get; set; } = new();
        public List<string> Followers { get; set; } = new();
        public int FollowersCount { get; set; } = 0;
        public int FollowingCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
