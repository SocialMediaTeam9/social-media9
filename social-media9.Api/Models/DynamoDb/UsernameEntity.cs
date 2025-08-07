using Amazon.DynamoDBv2.DataModel;
using social_media9.Api.Models.DynamoDb;

[DynamoDBTable("nexusphere-mvp-main-table")]
public class UsernameEntity : BaseEntity
{
    public UsernameEntity() { Type = "Username"; }

    public static UsernameEntity Create(string username)
    {
        var key = $"USERNAME#{username}";
        return new UsernameEntity { PK = key, SK = key, };
    }
}

