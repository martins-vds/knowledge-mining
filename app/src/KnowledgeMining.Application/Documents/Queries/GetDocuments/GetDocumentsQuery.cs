using FluentValidation;
using KnowledgeMining.Application.Common.Interfaces;
using MediatR;

namespace KnowledgeMining.Application.Documents.Queries.GetDocuments
{
    public record struct Document(string Name, IDictionary<string, string>? Tags);

    public readonly record struct GetDocumentsResponse(IEnumerable<Document> Documents, string? NextPage);

    public readonly record struct GetDocumentsQuery(string? SearchPrefix, int PageSize, string? ContinuationToken) : IRequest<GetDocumentsResponse>;

    public class GetDocumentsQueryValidator : AbstractValidator<GetDocumentsQuery>
    {
        public GetDocumentsQueryValidator()
        {
            RuleFor(q => q.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(5_000).WithMessage("Page size must a positive number between 1 and 5.000");
        }
    }

    public class GetDocumentsQueryHandler : IRequestHandler<GetDocumentsQuery, GetDocumentsResponse>
    {
        private readonly IStorageService _storageService;

        public GetDocumentsQueryHandler(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<GetDocumentsResponse> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
        {
            return await _storageService.GetDocuments(request.SearchPrefix, request.PageSize, request.ContinuationToken, cancellationToken);
        }
    }
}
