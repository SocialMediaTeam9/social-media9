using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System;

namespace social_media9.Api.Dtos
{
    public class CreatePostRequest
    {
        [Required]
        [StringLength(500)]
        public string Content { get; set; }
        public IFormFile? MediaFile { get; set; }
        public string MediaType { get; set; } // Optional: validate "image", "video"
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
}