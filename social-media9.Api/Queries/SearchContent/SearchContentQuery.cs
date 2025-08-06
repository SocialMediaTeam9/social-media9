using System.Collections.Generic;
using MediatR;
using social_media9.Api.Dtos;

namespace social_media9.Api.Queries.SearchContent
{
    public class SearchContentQuery : IRequest<IEnumerable<PostSearchResultDto>>
    {
        public string Query { get; }
        public int Limit { get; }

        public SearchContentQuery(string query, int limit)
        {
            Query = query;
            Limit = limit;
        }
    }
}