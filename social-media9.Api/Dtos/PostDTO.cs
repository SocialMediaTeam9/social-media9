using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System;

namespace social_media9.Api.Dtos
{
    public class CreatePostRequest
    {
        [Required]
        [StringLength(500)]
        public string Content { get; set; } = string.Empty;
        public IFormFile? MediaFile { get; set; }
        public string? MediaType { get; set; } // Optional: validate "image", "video"

        public List<string>? AttachmentUrls { get; set; }
    }

    public class PostDTO
    {
        public Guid PostId { get; set; }
        public string Content { get; set; }
        public string? MediaUrl { get; set; }
        public string MediaType { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
     public class UserProfileDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsFollowing { get; set; }
    }
}