using Amazon.DynamoDBv2.DataModel;
using social_media9.Api.Models.DynamoDb;

[DynamoDBTable("nexusphere-mvp-main-table")]
public class TimelineItemEntity : BaseEntity
{
    public TimelineItemEntity() { Type = "TimelineItem"; }
    [DynamoDBProperty("AuthorUsername")] public string AuthorUsername { get; set; } = string.Empty;
    [DynamoDBProperty("PostContent")] public string PostContent { get; set; } = string.Empty;
    [DynamoDBProperty("AttachmentUrls")] public List<string> AttachmentUrls { get; set; } = new();
    [DynamoDBProperty("CreatedAt")] public DateTime CreatedAt { get; set; }

    [DynamoDBProperty("BoostedBy")]
    public string? BoostedBy { get; set; }
}

public record TimelineItemResponse(
    string PostId,
    string AuthorUsername,
    string Content,
    List<string> Attachments,
    DateTime CreatedAt,
    string? BoostedBy
);

public record PaginatedTimelineResponse(
    List<TimelineItemResponse> Items,
    string? NextCursor
);