namespace social_media9.Api.Dtos
{
    public class SearchResultDto
    {
        public string ResultType { get; set; } 

        // --- User Properties (nullable) ---
        public string? UserId { get; set; }
        public string? FullName { get; set; }
        public string? ProfilePictureUrl { get; set; }

        // --- Post Properties (nullable) ---
        public string? PostId { get; set; }
        public string? Content { get; set; }
        
        // --- Shared Properties ---
        public string Username { get; set; } 
        public DateTime CreatedAt { get; set; }
    }
}