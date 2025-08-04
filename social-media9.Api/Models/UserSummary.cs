using System.Text.Json.Serialization;

namespace social_media9.Api.Models;

public class UserSummary
{
  // [JsonPropertyName("userId")]
  // public string UserId { get; set; }

  // [JsonPropertyName("username")]
  // public string Username { get; set; }

  [JsonPropertyName("fullName")] // Important for better search quality
  public string FullName { get; set; }

  // [JsonPropertyName("profilePictureUrl")]
  // public string ProfilePictureUrl { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
    
}