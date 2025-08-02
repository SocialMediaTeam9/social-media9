using System;

namespace social_media9.Api.Models
{
    public class UserProfile
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string GoogleId { get; set; } = string.Empty;
    }
}