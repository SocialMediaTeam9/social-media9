using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Text.Json;

namespace social_media9.Api.Models.ActivityPub;

// Model for the WebFinger JSON response (JRD - JSON Resource Descriptor)
public record WebFingerLink(
    [property: JsonPropertyName("rel")] string Rel,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("href")] string Href
);

public record WebFingerResponse(
    [property: JsonPropertyName("subject")] string Subject,
    [property: JsonPropertyName("aliases")] List<string> Aliases,
    [property: JsonPropertyName("links")] List<WebFingerLink> Links
);

// Model for the Public Key inside the Actor object
public record ActorPublicKey(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("owner")] string Owner,
    [property: JsonPropertyName("publicKeyPem")] string PublicKeyPem
);

// Model for the Actor JSON response
public record ActorResponse(
    [property: JsonPropertyName("@context")] IEnumerable<object> Context,
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("preferredUsername")] string? PreferredUsername,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("inbox")] string? Inbox,
    [property: JsonPropertyName("outbox")] string? Outbox,
    [property: JsonPropertyName("followers")] string? Followers,
    [property: JsonPropertyName("following")] string? Following,
    [property: JsonPropertyName("publicKey")] ActorPublicKey PublicKey,
    [property: JsonPropertyName("icon")] ActorImage? Icon,
    [property: JsonPropertyName("image")] ActorImage? Image,
    [property: JsonPropertyName("manuallyApprovesFollowers")] bool ManuallyApprovesFollowers,
    [property: JsonPropertyName("summary")] string? Summary,
    [property: JsonPropertyName("discoverable")] bool Discoverable
);

public record ActorImage(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("mediaType")] string MediaType,
    [property: JsonPropertyName("url")] string Url
);



public record OrderedCollectionPage
{
    // [JsonPropertyName("@context")]
    // public JsonElement Context { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "OrderedCollectionPage";

    [JsonPropertyName("next")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Next { get; set; }

    [JsonPropertyName("partOf")]
    public string PartOf { get; set; } = string.Empty;

    [JsonPropertyName("orderedItems")]
    public List<JsonElement> OrderedItems { get; set; } = new();
}

// Represents the top-level collection resource
public record OrderedCollection
{
    // [JsonPropertyName("@context")]
    // public JsonElement Context { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "OrderedCollection";

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    [JsonPropertyName("first")]
    public string First { get; set; } = string.Empty;
}