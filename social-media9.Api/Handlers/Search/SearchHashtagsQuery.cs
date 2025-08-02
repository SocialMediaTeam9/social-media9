using MediatR;
using social_media9.Api.Data;
using social_media9.Api.Models;

namespace social_media9.Api.Handlers.Search;

public record SearchHashtagsQuery(string Tag, int Limit) : IRequest<IEnumerable<VideoPostSummary>>;

public class SearchHashtagsQueryHandler : IRequestHandler<SearchHashtagsQuery, IEnumerable<VideoPostSummary>>
{
  private readonly ISearchRepository _repository;

  public SearchHashtagsQueryHandler(ISearchRepository repository)
  {
    _repository = repository;
  }

  public Task<IEnumerable<VideoPostSummary>> Handle(SearchHashtagsQuery request, CancellationToken cancellationToken)
  {
    return _repository.SearchHashtagsAsync(request.Tag, request.Limit, cancellationToken);
  }
}