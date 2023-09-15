using FluentValidation;
using KnowledgeMining.Application.Common.Interfaces;
using KnowledgeMining.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMining.Application.Documents.Queries.GetDocumentMetadata
{
    public readonly record struct GetDocumentMetadataQuery(string DocumentId) : IRequest<DocumentMetadata>;

    public class GetDocumentMetadataQueryValidator : AbstractValidator<GetDocumentMetadataQuery>
    {
        public GetDocumentMetadataQueryValidator()
        {
            RuleFor(q => q.DocumentId).NotEmpty().WithMessage("Document id must be provided");
        }
    }

    public class GetDocumentMetadataQueryHandler : IRequestHandler<GetDocumentMetadataQuery, DocumentMetadata>
    {
        private readonly ISearchService _searchService;

        public GetDocumentMetadataQueryHandler(ISearchService searchService)
        {
            _searchService = searchService;
        }

        public Task<DocumentMetadata> Handle(GetDocumentMetadataQuery request, CancellationToken cancellationToken)
        {
            return _searchService.GetDocumentDetails(request.DocumentId, cancellationToken);
        }
    }
}
