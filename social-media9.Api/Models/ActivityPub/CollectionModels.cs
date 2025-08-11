using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Text.Json;

namespace social_media9.Api.Models.ActivityPub;

public class ActivityPubCollectionPage
{
    [JsonPropertyName("@context")]
    public object Context { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("partOf")]
    public string PartOf { get; set; }

    [JsonPropertyName("orderedItems")]
    public List<object> OrderedItems { get; set; }

    [JsonPropertyName("next")]
    public string Next { get; set; }
}

public class ActivityPubCollection
{
    [JsonPropertyName("@context")]
    public object Context { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    [JsonPropertyName("first")]
    public string First { get; set; }
}