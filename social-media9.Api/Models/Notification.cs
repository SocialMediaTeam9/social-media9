using System.Text.Json.Serialization;

namespace social_media9.Api.Models;

public class Notification
{
  [JsonPropertyName("notificationId")]
  public string NotificationId { get; set; }

  [JsonPropertyName("userId")]
  public string UserId { get; set; }

  [JsonPropertyName("type")]
  public string Type { get; set; } // "like", "comment", "follow"

  [JsonPropertyName("message")]
  public string Message { get; set; }

  [JsonPropertyName("relatedContentId")]
  public string? RelatedContentId { get; set; }

  [JsonPropertyName("isRead")]
  public bool IsRead { get; set; }

  [JsonPropertyName("createdAt")]
  public DateTime CreatedAt { get; set; }
}