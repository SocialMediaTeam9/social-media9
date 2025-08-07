using MediatR;
using social_media9.Api.Dtos;
using System.Collections.Generic;

namespace social_media9.Api.Queries.SearchAll
{
    public record SearchAllQuery(string Query, int Limit) : IRequest<IEnumerable<SearchResultDto>>;
}