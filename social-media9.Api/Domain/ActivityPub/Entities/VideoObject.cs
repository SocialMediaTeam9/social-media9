namespace social_media9.Api.Domain.ActivityPub.Entities;

public class VideoObject
{
    public string Type => "Video";
    public string Content { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Attribution { get; set; } = string.Empty;
    public DateTime Published { get; set; } = DateTime.UtcNow;
}