using System.ComponentModel.DataAnnotations;

namespace social_media9.Api.Dtos
{
    public class AddCommentRequest
    {
        [Required]
        public string Content { get; set; }
    }
}
