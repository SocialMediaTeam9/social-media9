using System.Collections.Generic;
using MediatR;
using social_media9.Api.Dtos;

namespace social_media9.Api.Queries.SearchHashtags
{
    public class SearchHashtagsQuery : IRequest<IEnumerable<PostSearchResultDto>>
    {
        public string Query { get; }
        public int Limit { get; }

        public SearchHashtagsQuery(string hashtagQuery, int limit)
        {
            Query = hashtagQuery;
            Limit = limit;
        }
    }
}