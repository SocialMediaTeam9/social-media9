using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.Extensions.Options;
using social_media9.Api.Models;
using social_media9.Api.Configurations;

namespace social_media9.Api.Data
{
    // public class DynamoDbContext
    // {
    //     public DynamoDBContext Context { get; }

    //     public DynamoDbContext(IOptions<DynamoDbSettings> settings)
    //     {
    //         var config = new AmazonDynamoDBConfig { RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(settings.Value.Region) };
    //         var client = new AmazonDynamoDBClient(settings.Value.AccessKey, settings.Value.SecretKey, config);
    //         Context = new DynamoDBContext(client);
    //     }
    // }
    public class DynamoDbContext
{
    public DynamoDBContext Context { get; }

    public DynamoDbContext(IAmazonDynamoDB client)
    {
        Context = new DynamoDBContext(client);
    }
}
}
