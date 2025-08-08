namespace social_media9.Api.Domain.ActivityPub.Entities;

public class Activity
{
    public string Type { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public object Object { get; set; } = default!;
    public DateTime Published { get; set; } = DateTime.UtcNow;
    public string Id { get; set; } = string.Empty;
}