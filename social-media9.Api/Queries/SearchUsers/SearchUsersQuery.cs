using System.Collections.Generic;
using MediatR;
using social_media9.Api.Dtos;

namespace social_media9.Api.Queries.SearchUsers
{
    public class SearchUsersQuery : IRequest<IEnumerable<UserSearchResultDto>>
    {
        public string Query { get; }
        public int Limit { get; }

        public SearchUsersQuery(string query, int limit)
        {
            Query = query;
            Limit = limit;
        }
    }
}