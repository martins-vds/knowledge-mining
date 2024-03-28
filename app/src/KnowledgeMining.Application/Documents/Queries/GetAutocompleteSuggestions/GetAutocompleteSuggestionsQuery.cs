using KnowledgeMining.Application.Common.Interfaces;
using MediatR;

namespace KnowledgeMining.Application.Documents.Queries.GetAutocompleteSuggestions
{
    public readonly record struct GetAutocompleteSuggestionsQuery(string SearchQuery) : IRequest<IEnumerable<string>>;

    public class GetAutocompleteSuggestionsQueryHandler : IRequestHandler<GetAutocompleteSuggestionsQuery, IEnumerable<string>>
    {
        private readonly ISearchService _searchService;

        public GetAutocompleteSuggestionsQueryHandler(ISearchService searchService)
        {
            _searchService = searchService;
        }

        public async Task<IEnumerable<string>> Handle(GetAutocompleteSuggestionsQuery request, CancellationToken cancellationToken)
        {
            return await _searchService.Autocomplete(request.SearchQuery, true, cancellationToken);
        }
    }
}
