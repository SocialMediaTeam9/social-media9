namespace social_media9.Api.Data
{
    public class DynamoDbSettings
    {
        public string Region { get; set; } = string.Empty;
        public string ServiceUrl { get; set; } = string.Empty;
        public string UsersTableName { get; set; } = string.Empty;
        public string FollowsTableName { get; set; } = string.Empty;
    }
}