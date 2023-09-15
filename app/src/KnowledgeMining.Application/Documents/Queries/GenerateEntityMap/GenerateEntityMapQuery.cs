using FluentValidation;
using KnowledgeMining.Application.Common.Interfaces;
using KnowledgeMining.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMining.Application.Documents.Queries.GenerateEntityMap
{
    public readonly record struct GenerateEntityMapQuery(string? SearchText, IEnumerable<string> Facets, int MaxLevels, int MaxNodes) : IRequest<EntityMap>;

    public class GenerateEntityMapQueryValidator : AbstractValidator<GenerateEntityMapQuery>
    {
        public GenerateEntityMapQueryValidator()
        {
            RuleFor(r => r.MaxLevels).GreaterThan(0).WithMessage("Max levels must be a positive number");
            RuleFor(r => r.MaxNodes).GreaterThan(0).WithMessage("Max nodes must be a positive number");
        }
    }

    public class GenerateEntityMapQueryHandler : IRequestHandler<GenerateEntityMapQuery, EntityMap>
    {
        private readonly ISearchService _searchService;

        public GenerateEntityMapQueryHandler(ISearchService searchService)
        {
            _searchService = searchService;
        }

        public Task<EntityMap> Handle(GenerateEntityMapQuery request, CancellationToken cancellationToken)
        {
            return _searchService.GenerateEntityMap(request.SearchText, request.Facets, request.MaxLevels, request.MaxNodes, cancellationToken);
        }
    }
}
