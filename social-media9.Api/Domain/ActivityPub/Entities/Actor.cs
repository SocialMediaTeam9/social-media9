namespace social_media9.Api.Domain.ActivityPub.Entities;

public class Actor
{
    public string Id { get; set; } = string.Empty;
    public string Type => "Person";
    public string PreferredUsername { get; set; } = string.Empty;
    public string Inbox { get; set; } = string.Empty;
    public string Outbox { get; set; } = string.Empty;
    public PublicKey PublicKey { get; set; } = new();
}

public class PublicKey
{
    public string Id { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string PublicKeyPem { get; set; } = string.Empty;
}