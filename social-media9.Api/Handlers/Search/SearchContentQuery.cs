using MediatR;
using social_media9.Api.Data;
using social_media9.Api.Models;

namespace social_media9.Api.Handlers.Search;

// Using a record for a simple, immutable query object
public record SearchContentQuery(string Query, int Limit) : IRequest<IEnumerable<VideoPostSummary>>;

public class SearchContentQueryHandler : IRequestHandler<SearchContentQuery, IEnumerable<VideoPostSummary>>
{
  private readonly ISearchRepository _repository;

  public SearchContentQueryHandler(ISearchRepository repository)
  {
    _repository = repository;
  }

  public Task<IEnumerable<VideoPostSummary>> Handle(SearchContentQuery request, CancellationToken cancellationToken)
  {
    return _repository.SearchContentAsync(request.Query, request.Limit, cancellationToken);
  }
}