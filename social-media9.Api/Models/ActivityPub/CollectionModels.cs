using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Text.Json;

namespace social_media9.Api.Models.ActivityPub;

public class ActivityPubCollectionPage
{
    [JsonPropertyName("@context")]
    public object Context { get; set; }

    public string Id { get; set; }
    public string Type { get; set; }
    public string PartOf { get; set; }
    public List<object> OrderedItems { get; set; }
    public string Next { get; set; }
}

public class ActivityPubCollection
{
    [JsonPropertyName("@context")]
    public object Context { get; set; }

    public string Id { get; set; }
    public string Type { get; set; }
    public int TotalItems { get; set; }
    public string First { get; set; }
}