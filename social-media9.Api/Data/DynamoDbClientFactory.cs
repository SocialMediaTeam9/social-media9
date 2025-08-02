using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Options;

namespace social_media9.Api.Data
{
    public class DynamoDbClientFactory
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly DynamoDbSettings _settings;

        public DynamoDbClientFactory(IAmazonDynamoDB dynamoDbClient, IDynamoDBContext dynamoDbContext, IOptions<DynamoDbSettings> settings)
        {
            _dynamoDbClient = dynamoDbClient;
            _dynamoDbContext = dynamoDbContext;
            _settings = settings.Value;
        }

        public IAmazonDynamoDB GetClient() => _dynamoDbClient;
        public IDynamoDBContext GetContext() => _dynamoDbContext;
        public DynamoDbSettings GetSettings() => _settings;
    }
}
