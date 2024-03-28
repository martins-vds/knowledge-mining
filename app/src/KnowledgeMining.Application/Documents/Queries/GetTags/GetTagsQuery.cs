using KnowledgeMining.Application.Common.Options;
using KnowledgeMining.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;

namespace KnowledgeMining.Application.Documents.Queries.GetTags
{
    public record GetTagsQuery() : IRequest<DocumentTag[]>;

    public class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, DocumentTag[]>
    {
        private readonly IOptions<StorageOptions> _storageOptions;

        public GetTagsQueryHandler(IOptions<StorageOptions> storageOptions)
        {
            _storageOptions = storageOptions;
        }

        public Task<DocumentTag[]> Handle(GetTagsQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_storageOptions.Value.Tags);
        }
    }
}
