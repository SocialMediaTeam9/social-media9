using System.Text.Json.Serialization;

namespace social_media9.Api.Models;

public class VideoPostSummary
{
  [JsonPropertyName("contentId")]
  public string ContentId { get; set; }

  [JsonPropertyName("userId")]
  public string UserId { get; set; }

  [JsonPropertyName("username")]
  public string Username { get; set; }

  [JsonPropertyName("thumbnailUrl")]
  public string ThumbnailUrl { get; set; }

  [JsonPropertyName("title")]
  public string Title { get; set; }

  [JsonPropertyName("description")] // Needed for keyword searching
  public string Description { get; set; }

  [JsonPropertyName("hashtags")] // Needed for hashtag searching
  public List<string> Hashtags { get; set; } = new();

  [JsonPropertyName("likesCount")]
  public int LikesCount { get; set; }

  [JsonPropertyName("commentsCount")]
  public int CommentsCount { get; set; }
}