namespace social_media9.Api.Configurations
{
    public class ElasticsearchSettings
    {
        public string Uri { get; set; } = string.Empty;
        public string UsersIndex { get; set; } = string.Empty;
        public string PostsIndex { get; set; } = string.Empty;
    }
}