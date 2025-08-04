using Amazon.DynamoDBv2.DataModel;
using social_media9.Api.Models.DynamoDb;

[DynamoDBTable("nexusphere-mvp-main-table")]
public class UsernameEntity : BaseEntity
{
    public UsernameEntity() { Type = "Username"; }

    [DynamoDBProperty("UserId")]
    public string UserId { get; set; } = string.Empty;

    public static UsernameEntity Create(string username, string userId)
    {
        var key = $"USERNAME#{username}";
        return new UsernameEntity { PK = key, SK = key, UserId = userId };
    }
}

