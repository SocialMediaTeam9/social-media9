using System;
using System.Collections.Generic;

namespace social_media9.Api.Dtos
{
    public class PostSearchResultDto
    {
        public string PostId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Content { get; set; }
        public List<string> Hashtags { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}