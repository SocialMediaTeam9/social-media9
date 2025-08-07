using Amazon.DynamoDBv2.DataModel;

namespace social_media9.Api.Models.DynamoDb;

public abstract class BaseEntity
{
    [DynamoDBHashKey("PK")]
    public string PK { get; set; } = string.Empty;

    [DynamoDBRangeKey("SK")]
    public string SK { get; set; } = string.Empty;

    [DynamoDBProperty("Type")]
    public string Type { get; protected set; } = string.Empty;

    [DynamoDBGlobalSecondaryIndexHashKey("GSI1PK")]
    public string? GSI1PK { get; set; }

    [DynamoDBGlobalSecondaryIndexRangeKey("GSI1SK")]
    public string? GSI1SK { get; set; }

    [DynamoDBHashKey]
    public string PartitionKey { get; set; } = string.Empty;

    [DynamoDBRangeKey]
    public string SortKey { get; set; } = string.Empty;
}