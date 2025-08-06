using System;
using System.Collections.Generic;
using Nest;

namespace social_media9.Api.Models
{
    [ElasticsearchType(IdProperty = nameof(PostId))]
    public class PostSearchDocument
    {
        public string PostId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }

        [Text(Analyzer = "standard")]
        public string Content { get; set; }

        [Keyword] // Keyword type is ideal for exact matching on hashtags
        public List<string> Hashtags { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }
}