using MediatR;
using social_media9.Api.Data;
using social_media9.Api.Models;

namespace social_media9.Api.Handlers.Search;

public record SearchUsersQuery(string Query, int Limit) : IRequest<IEnumerable<UserSummary>>;

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, IEnumerable<UserSummary>>
{
  private readonly ISearchRepository _repository;

  public SearchUsersQueryHandler(ISearchRepository repository)
  {
    _repository = repository;
  }

  public Task<IEnumerable<UserSummary>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
  {
    return _repository.SearchUsersAsync(request.Query, request.Limit, cancellationToken);
  }
}