using Nest;

namespace social_media9.Api.Models
{
    // This attribute tells NEST which index this document belongs to,
    // though we'll specify it in queries for clarity.
    [ElasticsearchType(IdProperty = nameof(UserId))]
    public class UserSearchDocument
    {
        public string UserId { get; set; }

        [Text(Analyzer = "standard")]
        public string Username { get; set; }

        [Text(Analyzer = "standard")]
        public string FullName { get; set; }
        
        public string ProfilePictureUrl { get; set; }
    }
}