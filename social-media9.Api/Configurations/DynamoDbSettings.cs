namespace social_media9.Api.Configurations
{
    // public class DynamoDbSettings
    // {
        // public string AccessKey { get; set; }
        // public string SecretKey { get; set; }
        // public string Region { get; set; }
    // }
        public class DynamoDbSettings
    {
        public string Region { get; set; } = string.Empty;
        public string ServiceUrl { get; set; } = string.Empty;
        public string UsersTableName { get; set; } = string.Empty;
        public string FollowsTableName { get; set; } = string.Empty;
    }
}
