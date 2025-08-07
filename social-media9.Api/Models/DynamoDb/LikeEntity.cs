using Amazon.DynamoDBv2.DataModel;
using social_media9.Api.Models.DynamoDb;

[DynamoDBTable("nexusphere-mvp-main-table")]
public class LikeEntity : BaseEntity
{
    public LikeEntity() { Type = "Like"; }

    [DynamoDBProperty("LikerUsername")]
    public string LikerUsername { get; set; } = string.Empty;

    [DynamoDBProperty("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


[DynamoDBTable("PeerspaceTable")]
public class BoostEntity : BaseEntity
{
    public BoostEntity() { Type = "Boost"; }

    [DynamoDBProperty("BoosterUsername")]
    public string BoosterUsername { get; set; } = string.Empty;

    [DynamoDBProperty("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}