namespace social_media9.Api.Models
{
    public class UserSummary
    {
        public UserSummary()
        {
        }

        public UserSummary(string UserId, string Username, string ActorUrl, string? ProfilePictureUrl)
        {
            this.UserId = UserId;
            this.Username = Username;
            this.ActorUrl = ActorUrl;
            this.ProfilePictureUrl = ProfilePictureUrl;
        }

        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public string ActorUrl { get; set; } = string.Empty;
    }
}